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

        public void UpdateOrder(Order order)
        {
            try
            {
                var strategy = dbContext.Database.CreateExecutionStrategy();
                
                strategy.Execute(() =>
                {
                    using (var transaction = dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var existingOrder = dbContext.Orders
                                .Include(o => o.OrderDetails)
                                .FirstOrDefault(o => o.Id == order.Id);

                            if (existingOrder == null)
                            {
                                throw new InvalidOperationException($"Order with ID {order.Id} not found");
                            }

                            // Update order properties
                            existingOrder.FirstName = order.FirstName;
                            existingOrder.LastName = order.LastName;
                            existingOrder.Email = order.Email;
                            existingOrder.Phone = order.Phone;
                            existingOrder.Address = order.Address;
                            existingOrder.OrderTotal = order.OrderTotal;

                            dbContext.SaveChanges();
                            transaction.Commit();
                            
                            logger.LogInformation($"Order {order.Id} updated successfully");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            logger.LogError(ex, $"Error updating order {order.Id}");
                            throw;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error in UpdateOrder method for order {order.Id}");
                throw;
            }
        }

        public void DeleteOrder(int orderId)
        {
            try
            {
                var strategy = dbContext.Database.CreateExecutionStrategy();
                
                strategy.Execute(() =>
                {
                    using (var transaction = dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var order = dbContext.Orders
                                .Include(o => o.OrderDetails)
                                .FirstOrDefault(o => o.Id == orderId);

                            if (order == null)
                            {
                                throw new InvalidOperationException($"Order with ID {orderId} not found");
                            }

                            // Remove order details first
                            if (order.OrderDetails != null && order.OrderDetails.Any())
                            {
                                dbContext.OrderDetails.RemoveRange(order.OrderDetails);
                            }

                            // Remove the order
                            dbContext.Orders.Remove(order);
                            dbContext.SaveChanges();
                            
                            transaction.Commit();
                            logger.LogInformation($"Order {orderId} deleted successfully");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            logger.LogError(ex, $"Error deleting order {orderId}");
                            throw;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error in DeleteOrder method for order {orderId}");
                throw;
            }
        }

        public Order CreateNewOrder()
        {
            return new Order
            {
                OrderPlaced = DateTime.Now,
                OrderTotal = 0
            };
        }

        public void CreateAdminOrder(Order order)
        {
            try
            {
                var strategy = dbContext.Database.CreateExecutionStrategy();
                
                strategy.Execute(() =>
                {
                    using (var transaction = dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            // Set order properties for admin-created order
                            order.OrderPlaced = DateTime.Now;
                            
                            logger.LogInformation($"Creating admin order for: {order.Email}, Total: {order.OrderTotal}");
                            
                            // Save order
                            dbContext.Orders.Add(order);
                            dbContext.SaveChanges();
                            
                            // Commit the transaction
                            transaction.Commit();
                            logger.LogInformation($"Admin order {order.Id} created successfully");
                        }
                        catch (Exception ex)
                        {
                            // Roll back if there's an error
                            transaction.Rollback();
                            logger.LogError(ex, "Error saving admin order during transaction");
                            throw;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in CreateAdminOrder method");
                throw;
            }
        }
    }
}
