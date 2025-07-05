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
    }
}