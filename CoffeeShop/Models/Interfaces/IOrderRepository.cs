namespace CoffeeShop.Models.Interfaces
{
    public interface IOrderRepository
    {
        void PlaceOrder(Order order);
        List<Order> GetOrdersByUser(string userId);
        Order? GetOrderById(int orderId);
        List<Order> GetAllOrders();
    }
}
