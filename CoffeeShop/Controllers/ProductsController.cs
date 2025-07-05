using CoffeeShop.Models;
using CoffeeShop.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public IActionResult Shop()
        {
            var products = _productRepository.GetAllProducts();
            return View(products);
        }

        public IActionResult Detail(int id)
        {
            var product = _productRepository.GetProductDetails(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public IActionResult Trending()
        {
            var products = _productRepository.GetTrendingProducts();
            return View(products);
        }

        // CRUD operations
        [Authorize]
        public IActionResult Manage()
        {
            var products = _productRepository.GetAllProducts();
            return View(products);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                _productRepository.AddProduct(product);
                TempData["Success"] = "Product created successfully";
                return RedirectToAction(nameof(Manage));
            }
            return View(product);
        }

        [Authorize]
        public IActionResult Edit(int id)
        {
            var product = _productRepository.GetProductDetails(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Edit(int id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _productRepository.UpdateProduct(product);
                    TempData["Success"] = "Product updated successfully";
                }
                catch (Exception)
                {
                    if (!_productRepository.ProductExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Manage));
            }
            return View(product);
        }

        [Authorize]
        public IActionResult Delete(int id)
        {
            var product = _productRepository.GetProductDetails(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _productRepository.DeleteProduct(id);
                TempData["Success"] = "Product deleted successfully";
                return RedirectToAction(nameof(Manage));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Manage));
            }
        }
    }
}
