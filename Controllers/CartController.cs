using Microsoft.AspNetCore.Mvc;
using BaiTapThucHanh.Data;
using BaiTapThucHanh.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BaiTapThucHanh.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult AddToCart(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.ProductId == id);

            if (item == null)
            {
                cart.Add(new CartItem {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    Quantity = 1
                });
            }
            else
            {
                item.Quantity++;
            }

            SaveCartSession(cart);
            return Json(new { success = true, count = cart.Sum(i => i.Quantity) });
        }

        public IActionResult GetCartCount()
        {
            var cart = GetCartItems();
            return Json(new { count = cart.Sum(i => i.Quantity) });
        }

        private List<CartItem> GetCartItems()
        {
            var sessionData = HttpContext.Session.GetString("Cart");
            return sessionData == null ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(sessionData) ?? new List<CartItem>();
        }

        private void SaveCartSession(List<CartItem> cart)
        {
            var sessionData = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("Cart", sessionData);
        }
    }
}