using CoffeeShop.Models;
using CoffeeShop.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AdminDashboardController> _logger;

        public AdminDashboardController(
            IOrderRepository orderRepository,
            UserManager<IdentityUser> userManager,
            ILogger<AdminDashboardController> logger)
        {
            _orderRepository = orderRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                var orders = _orderRepository.GetAllOrders();
                ViewBag.OrderCount = orders.Count;
                ViewBag.TotalRevenue = orders.Sum(o => o.OrderTotal);
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                ViewBag.OrderCount = 0;
                ViewBag.TotalRevenue = 0;
                TempData["ErrorMessage"] = "Error loading dashboard data.";
                return View();
            }
        }

        public IActionResult Orders()
        {
            try
            {
                var orders = _orderRepository.GetAllOrders();
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin orders");
                return View(new List<Order>());
            }
        }

        public IActionResult OrderDetails(int id)
        {
            try
            {
                var order = _orderRepository.GetOrderById(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("Orders");
                }
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading order details for ID: {id}");
                TempData["ErrorMessage"] = "Error loading order details.";
                return RedirectToAction("Orders");
            }
        }

        public IActionResult CreateOrder()
        {
            var order = _orderRepository.CreateNewOrder();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateOrder(Order order)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _orderRepository.CreateAdminOrder(order);
                    TempData["SuccessMessage"] = "Order created successfully.";
                    return RedirectToAction("Orders");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                ModelState.AddModelError("", "Error creating order. Please try again.");
            }
            return View(order);
        }

        public IActionResult EditOrder(int id)
        {
            try
            {
                var order = _orderRepository.GetOrderById(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("Orders");
                }
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading order for editing: {id}");
                TempData["ErrorMessage"] = "Error loading order for editing.";
                return RedirectToAction("Orders");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditOrder(Order order)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _orderRepository.UpdateOrder(order);
                    TempData["SuccessMessage"] = "Order updated successfully.";
                    return RedirectToAction("Orders");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order: {order.Id}");
                ModelState.AddModelError("", "Error updating order. Please try again.");
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteOrder(int id)
        {
            try
            {
                _orderRepository.DeleteOrder(id);
                TempData["SuccessMessage"] = "Order deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting order: {id}");
                TempData["ErrorMessage"] = "Error deleting order. Please try again.";
            }
            return RedirectToAction("Orders");
        }
    }
}