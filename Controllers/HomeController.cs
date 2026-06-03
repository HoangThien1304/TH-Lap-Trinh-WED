using System.Diagnostics;
using BaiTapThucHanh.Models;
using BaiTapThucHanh.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BaiTapThucHanh.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Bổ sung tham số int? categoryId để nhận bộ lọc danh mục từ giao diện gửi lên
        public IActionResult Index(int? categoryId)
        {
            // 1. Lấy toàn bộ danh sách danh mục để làm thanh lọc
            ViewBag.Categories = _context.Categories.ToList();

            // Lưu lại categoryId hiện tại đang lọc để giữ trạng thái active trên giao diện
            ViewBag.CurrentCategoryId = categoryId;

            // 2. Lấy danh sách sản phẩm (kèm liên kết Category)
            IQueryable<Product> productsQuery = _context.Products.Include(p => p.Category);

            // Nếu người dùng có chọn 1 danh mục cụ thể thì lọc theo danh mục đó
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = productsQuery.ToList();
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
    }
}