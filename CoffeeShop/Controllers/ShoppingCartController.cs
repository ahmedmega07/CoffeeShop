using CoffeeShop.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Controllers
{
    public class ShoppingCartController : Controller
    {
        private IShoppingCartRepository shoppingCartRepository;
        private IProductRepository productRepository;
        public ShoppingCartController(IShoppingCartRepository shoppingCartRepository, IProductRepository productRepository)
        {
            this.shoppingCartRepository = shoppingCartRepository;
            this.productRepository = productRepository;
        }
        public IActionResult Index()
        {
            var items = shoppingCartRepository.GetShoppingCartItems();
            shoppingCartRepository.ShoppingCartItems = items;
            ViewBag.CartTotal = shoppingCartRepository.GetShoppingCartTotal();
            return View(items);
        }

        public RedirectToActionResult AddToShoppingCart(int pId)
        {
            var product = productRepository.GetAllProducts().FirstOrDefault(p => p.Id == pId);
            if (product != null)
            {
                shoppingCartRepository.AddToCart(product);
                int cartCount = shoppingCartRepository.GetShoppingCartItems().Count;
                HttpContext.Session.SetInt32("CartCount", cartCount);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult AddToShoppingCartAjax(int pId)
        {
            try
            {
                var product = productRepository.GetAllProducts().FirstOrDefault(p => p.Id == pId);
                if (product != null)
                {
                    shoppingCartRepository.AddToCart(product);
                    var cartItems = shoppingCartRepository.GetShoppingCartItems();
                    int cartCount = cartItems.Count;
                    decimal cartTotal = shoppingCartRepository.GetShoppingCartTotal();
                    
                    HttpContext.Session.SetInt32("CartCount", cartCount);

                    return Json(new { 
                        success = true, 
                        cartCount = cartCount,
                        cartTotal = cartTotal.ToString("c"),
                        message = $"{product.Name} added to cart successfully!" 
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Product not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while adding the item to cart." });
            }
        }

        public RedirectToActionResult RemoveFromShoppingCart(int pId)
        {
            var product = productRepository.GetAllProducts().FirstOrDefault(p => p.Id == pId);
            if (product != null)
            {
                shoppingCartRepository.RemoveFromCart(product);
                int cartCount = shoppingCartRepository.GetShoppingCartItems().Count;
                HttpContext.Session.SetInt32("CartCount", cartCount);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult RemoveFromShoppingCartAjax(int pId)
        {
            try
            {
                var product = productRepository.GetAllProducts().FirstOrDefault(p => p.Id == pId);
                if (product != null)
                {
                    shoppingCartRepository.RemoveFromCart(product);
                    var cartItems = shoppingCartRepository.GetShoppingCartItems();
                    int cartCount = cartItems.Count;
                    decimal cartTotal = shoppingCartRepository.GetShoppingCartTotal();
                    
                    HttpContext.Session.SetInt32("CartCount", cartCount);

                    return Json(new { 
                        success = true, 
                        cartCount = cartCount,
                        cartTotal = cartTotal.ToString("c"),
                        message = "Item removed from cart successfully!" 
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Product not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while removing the item from cart." });
            }
        }

        public RedirectToActionResult IncreaseQuantity(int pId)
        {
            var items = shoppingCartRepository.GetShoppingCartItems();
            var item = items.FirstOrDefault(i => i.Product.Id == pId);
            
            if (item != null)
            {
                shoppingCartRepository.UpdateCartItemQuantity(pId, item.Qty + 1);
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult IncreaseQuantityAjax(int pId)
        {
            try
            {
                var items = shoppingCartRepository.GetShoppingCartItems();
                var item = items.FirstOrDefault(i => i.Product.Id == pId);
                
                if (item != null)
                {
                    int newQuantity = shoppingCartRepository.UpdateCartItemQuantity(pId, item.Qty + 1);
                    var cartItems = shoppingCartRepository.GetShoppingCartItems();
                    int cartCount = cartItems.Count;
                    decimal cartTotal = shoppingCartRepository.GetShoppingCartTotal();
                    
                    HttpContext.Session.SetInt32("CartCount", cartCount);

                    return Json(new { 
                        success = true, 
                        cartCount = cartCount,
                        cartTotal = cartTotal.ToString("c"),
                        newQuantity = newQuantity,
                        itemTotal = (item.Product.Price * newQuantity).ToString("c")
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Item not found in cart." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the quantity." });
            }
        }

        public RedirectToActionResult DecreaseQuantity(int pId)
        {
            var items = shoppingCartRepository.GetShoppingCartItems();
            var item = items.FirstOrDefault(i => i.Product.Id == pId);
            
            if (item != null && item.Qty > 1)
            {
                shoppingCartRepository.UpdateCartItemQuantity(pId, item.Qty - 1);
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult DecreaseQuantityAjax(int pId)
        {
            try
            {
                var items = shoppingCartRepository.GetShoppingCartItems();
                var item = items.FirstOrDefault(i => i.Product.Id == pId);
                
                if (item != null && item.Qty > 1)
                {
                    int newQuantity = shoppingCartRepository.UpdateCartItemQuantity(pId, item.Qty - 1);
                    var cartItems = shoppingCartRepository.GetShoppingCartItems();
                    int cartCount = cartItems.Count;
                    decimal cartTotal = shoppingCartRepository.GetShoppingCartTotal();
                    
                    HttpContext.Session.SetInt32("CartCount", cartCount);

                    return Json(new { 
                        success = true, 
                        cartCount = cartCount,
                        cartTotal = cartTotal.ToString("c"),
                        newQuantity = newQuantity,
                        itemTotal = (item.Product.Price * newQuantity).ToString("c")
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Cannot decrease quantity below 1." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the quantity." });
            }
        }

        public RedirectToActionResult UpdateQuantity(int pId, int quantity)
        {
            if (quantity > 0)
            {
                shoppingCartRepository.UpdateCartItemQuantity(pId, quantity);
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult UpdateQuantityAjax(int pId, int quantity)
        {
            try
            {
                if (quantity > 0)
                {
                    var items = shoppingCartRepository.GetShoppingCartItems();
                    var item = items.FirstOrDefault(i => i.Product.Id == pId);
                    
                    if (item != null)
                    {
                        int newQuantity = shoppingCartRepository.UpdateCartItemQuantity(pId, quantity);
                        var cartItems = shoppingCartRepository.GetShoppingCartItems();
                        int cartCount = cartItems.Count;
                        decimal cartTotal = shoppingCartRepository.GetShoppingCartTotal();
                        
                        HttpContext.Session.SetInt32("CartCount", cartCount);

                        return Json(new { 
                            success = true, 
                            cartCount = cartCount,
                            cartTotal = cartTotal.ToString("c"),
                            newQuantity = newQuantity,
                            itemTotal = (item.Product.Price * newQuantity).ToString("c")
                        });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Item not found in cart." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Quantity must be greater than 0." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the quantity." });
            }
        }
    }
}
