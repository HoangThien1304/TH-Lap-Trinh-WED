using BaiTapThucHanh.Data;
using BaiTapThucHanh.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace BaiTapThucHanh.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        // Tiêm AppDbContext để tương tác với cơ sở dữ liệu SQL Server
        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Xem danh sách danh mục
        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        // 2. Thêm mới danh mục (Giao diện GET)
        public IActionResult Add()
        {
            return View();
        }

        // 3. Xử lý thêm mới danh mục (POST)
        [HttpPost]
        public IActionResult Add(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // 4. Sửa danh mục (Giao diện GET)
        public IActionResult Update(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // 5. Xử lý sửa danh mục (POST)
        [HttpPost]
        public IActionResult Update(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(category);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // 6. Xóa danh mục (Giao diện GET)
        public IActionResult Delete(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // 7. Xử lý xóa danh mục (POST)
        [HttpPost, ActionName("DeleteConfirmed")]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}