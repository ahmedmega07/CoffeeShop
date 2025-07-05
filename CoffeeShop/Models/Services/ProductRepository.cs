using CoffeeShop.Data;
using CoffeeShop.Models.Interfaces;

namespace CoffeeShop.Models.Services;

public class ProductRepository : IProductRepository
{
    private readonly CoffeeShopDbContext _context;

    public ProductRepository(CoffeeShopDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Product> GetAllProducts()
    {
        return _context.Products.ToList();
    }

    public Product? GetProductDetails(int id)
    {
        return _context.Products.FirstOrDefault(p => p.Id == id);
    }

    public IEnumerable<Product> GetTrendingProducts()
    {
        return _context.Products.Where(p => p.IsTrendingProduct).ToList();
    }

    public void AddProduct(Product product)
    {
        _context.Products.Add(product);
        _context.SaveChanges();
    }

    public void UpdateProduct(Product product)
    {
        _context.Products.Update(product);
        _context.SaveChanges();
    }

    public void DeleteProduct(int id)
    {
        var product = _context.Products.Find(id);
        if (product != null)
        {
            // Check if the product is in any shopping carts
            bool inCart = _context.ShoppingCartItems.Any(s => s.Product.Id == id);
            
            // Check if the product is in any orders
            bool inOrder = _context.OrderDetails.Any(od => od.ProductId == id);
            
            if (inCart || inOrder)
            {
                // If the product is referenced elsewhere, consider soft delete or handle accordingly
                // For this implementation, we'll prevent deletion
                throw new InvalidOperationException("Cannot delete product because it's referenced in orders or shopping carts.");
            }
            
            _context.Products.Remove(product);
            _context.SaveChanges();
        }
    }

    public bool ProductExists(int id)
    {
        return _context.Products.Any(p => p.Id == id);
    }
}