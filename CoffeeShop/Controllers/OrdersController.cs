using CoffeeShop.Models;
using CoffeeShop.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CoffeeShop.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IShoppingCartRepository _shopCartRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderRepository orderRepository, 
            IShoppingCartRepository shopCartRepository,
            UserManager<IdentityUser> userManager,
            ILogger<OrdersController> logger)
        {
            _orderRepository = orderRepository;
            _shopCartRepository = shopCartRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Checkout()
        {
            try
            {
                // Check if there are items in the cart
                var items = _shopCartRepository.GetShoppingCartItems();
                if (items.Count == 0)
                {
                    TempData["Error"] = "Your cart is empty. Add some items before checkout.";
                    return RedirectToAction("Index", "ShoppingCart");
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                var order = new Order
                {
                    Email = user.Email
                };
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing checkout page");
                TempData["Error"] = "An error occurred while loading the checkout page. Please try again.";
                return RedirectToAction("Index", "ShoppingCart");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            try
            {
                // Check if there are items in the cart before processing
                var items = _shopCartRepository.GetShoppingCartItems();
                if (items.Count == 0)
                {
                    ModelState.AddModelError("", "Your cart is empty. Add some items before checkout.");
                    return View(order);
                }

                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        return RedirectToPage("/Account/Login", new { area = "Identity" });
                    }

                    // Ensure the email matches the logged-in user's email
                    order.Email = user.Email;
                    order.UserId = user.Id;

                    try
                    {
                        _orderRepository.PlaceOrder(order);
                        _shopCartRepository.ClearCart();
                        HttpContext.Session.SetInt32("CartCount", 0);
                        return RedirectToAction("CheckoutComplete", new { orderId = order.Id });
                    }
                    catch (DbUpdateException ex)
                    {
                        _logger.LogError(ex, "Database error when placing order");
                        ModelState.AddModelError("", "Error placing your order: Database error. Please try again later.");
                        return View(order);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error when placing order");
                        ModelState.AddModelError("", $"Error placing your order: {ex.Message}");
                        return View(order);
                    }
                }
                
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in checkout post");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                return View(order);
            }
        }
        
        public IActionResult CheckoutComplete(int orderId = 0)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        public IActionResult MyOrders()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }
                
                var orders = _orderRepository.GetOrdersByUser(userId);
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user orders");
                TempData["Error"] = "Failed to retrieve your orders. Please try again later.";
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult OrderDetails(int id)
        {
            try
            {
                var order = _orderRepository.GetOrderById(id);
                if (order == null)
                {
                    return NotFound();
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (order.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order details for ID: {0}", id);
                TempData["Error"] = "Failed to retrieve order details. Please try again later.";
                return RedirectToAction("MyOrders");
            }
        }
        
        [Authorize(Roles = "Admin")]
        public IActionResult AllOrders()
        {
            try
            {
                var orders = _orderRepository.GetAllOrders();
                return View("AllOrders", orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders");
                TempData["Error"] = "Failed to retrieve orders. Please try again later.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
