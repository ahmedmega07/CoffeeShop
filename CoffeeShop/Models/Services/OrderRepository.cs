using CoffeeShop.Data;
using CoffeeShop.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CoffeeShop.Models.Services
{
    public class OrderRepository : IOrderRepository
    {
        private readonly CoffeeShopDbContext dbContext;
        private readonly IShoppingCartRepository shoppingCartRepository;
        private readonly ILogger<OrderRepository> logger;
        
        public OrderRepository(CoffeeShopDbContext dbContext, IShoppingCartRepository shoppingCartRepository, ILogger<OrderRepository> logger)
        {
            this.dbContext = dbContext;
            this.shoppingCartRepository = shoppingCartRepository;
            this.logger = logger;
        }
        
        public void PlaceOrder(Order order)
        {
            try
            {
                // Get shopping cart items
                var shoppingCartItems = shoppingCartRepository.GetShoppingCartItems();
                if (shoppingCartItems == null || !shoppingCartItems.Any())
                {
                    throw new InvalidOperationException("Cannot place order with an empty cart");
                }

                // Set order properties
                order.OrderPlaced = DateTime.Now;
                order.OrderTotal = shoppingCartRepository.GetShoppingCartTotal();
                
                // Log for debugging
                logger.LogInformation($"Creating order for user: {order.UserId}, Total: {order.OrderTotal}");
                
                // Use execution strategy instead of a direct transaction
                var strategy = dbContext.Database.CreateExecutionStrategy();
                
                strategy.Execute(() =>
                {
                    using (var transaction = dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            // Save order first
                            dbContext.Orders.Add(order);
                            dbContext.SaveChanges();
                            
                            logger.LogInformation($"Order created with ID: {order.Id}");
                            
                            // Create order details
                            var orderDetails = new List<OrderDetail>();
                            foreach (var item in shoppingCartItems)
                            {
                                if (item.Product != null)
                                {
                                    var orderDetail = new OrderDetail
                                    {
                                        OrderId = order.Id,
                                        ProductId = item.Product.Id,
                                        Quantity = item.Qty,
                                        Price = item.Product.Price
                                    };
                                    orderDetails.Add(orderDetail);
                                }
                            }
                            
                            // Add all details at once
                            dbContext.OrderDetails.AddRange(orderDetails);
                            dbContext.SaveChanges();
                            
                            // Commit the transaction
                            transaction.Commit();
                            logger.LogInformation($"Order {order.Id} completed successfully with {orderDetails.Count} items");
                        }
                        catch (Exception ex)
                        {
                            // Roll back if there's an error
                            transaction.Rollback();
                            logger.LogError(ex, "Error saving order and details during transaction");
                            throw;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in PlaceOrder method");
                throw;
            }
        }

        public List<Order> GetOrdersByUser(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    logger.LogWarning("GetOrdersByUser called with null or empty userId");
                    return new List<Order>();
                }

                return dbContext.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .OrderByDescending(o => o.OrderPlaced)
                    .ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error getting orders for user {userId}");
                return new List<Order>();
            }
        }

        public Order? GetOrderById(int orderId)
        {
            try
            {
                return dbContext.Orders
                    .Where(o => o.Id == orderId)
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error getting order {orderId}");
                return null;
            }
        }
        
        public List<Order> GetAllOrders()
        {
            try
            {
                return dbContext.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .OrderByDescending(o => o.OrderPlaced)
                    .ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting all orders");
                return new List<Order>();
            }
        }
    }
}
