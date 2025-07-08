namespace CoffeeShop.Models.Interfaces
{
    public interface IOrderRepository
    {
        void PlaceOrder(Order order);
        List<Order> GetOrdersByUser(string userId);
        Order? GetOrderById(int orderId);
        List<Order> GetAllOrders();

        // New methods for admin order management
        void UpdateOrder(Order order);
        void DeleteOrder(int orderId);
        Order CreateNewOrder();
        void CreateAdminOrder(Order order); // New method for admin-created orders
    }
}
