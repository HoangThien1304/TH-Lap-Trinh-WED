using System.Diagnostics;
using BaiTapThucHanh.Models;
using BaiTapThucHanh.Data; // Đảm bảo đã khai báo để dùng AppDbContext
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // BẮT BUỘC có dòng này để dùng được hàm .Include()
using System.Linq;

namespace BaiTapThucHanh.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context; // Đổi sang dùng AppDbContext trực tiếp

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // ĐỔI TỪ p.CategoryId THÀNH p.Category
            var products = _context.Products.Include(p => p.Category).ToList();
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}