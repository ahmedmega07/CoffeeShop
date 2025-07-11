﻿using CoffeeShop.Models.Interfaces;
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

        public RedirectToActionResult UpdateQuantity(int pId, int quantity)
        {
            if (quantity > 0)
            {
                shoppingCartRepository.UpdateCartItemQuantity(pId, quantity);
            }
            
            return RedirectToAction("Index");
        }
    }
}
