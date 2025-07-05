namespace CoffeeShop.Models.Interfaces;

public interface IProductRepository
{
    IEnumerable<Product> GetAllProducts();
    IEnumerable<Product> GetTrendingProducts();
    Product? GetProductDetails(int id);
    
    // CRUD operations
    void AddProduct(Product product);
    void UpdateProduct(Product product);
    void DeleteProduct(int id);
    bool ProductExists(int id);
}
