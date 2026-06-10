using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using BaiTapThucHanh.Data;
using BaiTapThucHanh.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace BaiTapThucHanh.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private const string CART_KEY = "Cart";

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // Xem giỏ hàng
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // API: Thêm vào giỏ hàng (Gọi bằng AJAX)
        public IActionResult AddToCart(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return Json(new { success = false, message = "Sản phẩm không tồn tại" });

            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == id);

            if (item == null)
            {
                cart.Add(new CartItem
                {
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

            SaveCart(cart);
            return Json(new { success = true, count = cart.Sum(i => i.Quantity) });
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == id);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        // Cập nhật số lượng sản phẩm
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            if (quantity <= 0)
            {
                return Remove(id);
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == id);
            if (item != null)
            {
                item.Quantity = quantity;
                SaveCart(cart);
                return Json(new { success = true, total = item.Quantity * item.Price, cartTotal = cart.Sum(i => i.Quantity * i.Price) });
            }

            return Json(new { success = false });
        }

        // API: Lấy số lượng giỏ hàng hiện tại
        public IActionResult GetCartCount()
        {
            var cart = GetCart();
            return Json(new { count = cart.Sum(i => i.Quantity) });
        }

        // Helpers xử lý Session JSON
        private List<CartItem> GetCart()
        {
            var sessionData = HttpContext.Session.GetString(CART_KEY);
            if (sessionData == null) return new List<CartItem>();
            
            return JsonSerializer.Deserialize<List<CartItem>>(sessionData) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            var sessionData = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CART_KEY, sessionData);
        }
    }
}