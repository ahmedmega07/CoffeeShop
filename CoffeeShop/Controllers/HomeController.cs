using CoffeeShop.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Controllers;

public class HomeController : Controller
{
    private IProductRepository _productRepository;

    public HomeController(IProductRepository productRepository)
    {
        _productRepository = productRepository;

    }







    // GET
    public IActionResult Index()
    {
        var products = _productRepository.GetTrendingProducts();
        return View(products);

    }
}