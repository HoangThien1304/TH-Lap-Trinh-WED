using BaiTapThucHanh.Models;
using BaiTapThucHanh.Data; // Khai báo để sử dụng AppDbContext
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Linq; // Thêm thư viện này để truy vấn LINQ dữ liệu

namespace BaiTapThucHanh.Controllers
{
    public class ProductController : Controller
    {
        // Thay đổi từ Repository sang AppDbContext
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Constructor tiêm AppDbContext và môi trường Web
        public ProductController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================================
        // ĐỌC DANH SÁCH SẢN PHẨM (INDEX)
        // ==========================================
        public IActionResult Index()
        {
            // Lấy toàn bộ danh sách sản phẩm từ cơ sở dữ liệu SQL Server
            var products = _context.Products.ToList();
            return View(products);
        }

        // ==========================================
        // XEM CHI TIẾT SẢN PHẨM (DISPLAY)
        // ==========================================
        public IActionResult Display(int id)
        {
            // Tìm sản phẩm theo Id trong DB
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // ==========================================
        // 1. THÊM SẢN PHẨM (ADD)
        // ==========================================
        public IActionResult Add()
        {
            // Lấy danh sách danh mục từ bảng Categories để truyền vào DropdownList
            var categories = _context.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult Add(Product product)
        {
            if (ModelState.IsValid)
            {
                // Xử lý lưu ảnh nếu người dùng chọn file ảnh tải lên
                if (product.ImageFile != null)
                {
                    // Trỏ thẳng ra thư mục "images" nằm ở gốc dự án bằng ContentRootPath
                    string uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + product.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        product.ImageFile.CopyTo(fileStream);
                    }

                    product.ImageUrl = uniqueFileName;
                }

                // Thêm sản phẩm vào DbSet và lưu thay đổi xuống SQL Server
                _context.Products.Add(product);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            var categories = _context.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        // ==========================================
        // 2. CẬP NHẬT / SỬA SẢN PHẨM (UPDATE)
        // ==========================================
        public ActionResult Update(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = _context.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);

            return View(product);
        }

        [HttpPost]
        public IActionResult Update(Product product)
        {
            if (ModelState.IsValid)
            {
                // Tìm sản phẩm gốc hiện tại trong Database để lấy lại thông tin ảnh cũ (AsNoTracking giúp tránh xung đột thực thể)
                var existingProduct = _context.Products.AsNoTracking().FirstOrDefault(p => p.Id == product.Id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                if (product.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "images");

                    // Xóa file ảnh cũ trên ổ cứng (nếu có)
                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        string oldFilePath = Path.Combine(uploadsFolder, existingProduct.ImageUrl);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Lưu ảnh mới
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + product.ImageFile.FileName;
                    string newFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                    {
                        product.ImageFile.CopyTo(fileStream);
                    }

                    product.ImageUrl = uniqueFileName;
                }
                else
                {
                    // Nếu không tải ảnh mới, giữ nguyên tên ảnh cũ
                    product.ImageUrl = existingProduct.ImageUrl;
                }

                // Cập nhật thông tin mới vào Db và lưu lại
                _context.Products.Update(product);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            var categories = _context.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // ==========================================
        // 3. XÓA SẢN PHẨM (DELETE)
        // ==========================================
        public IActionResult Delete(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                // Xóa file ảnh vật lý trong thư mục gốc /images trước khi xóa trong DB
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "images", product.ImageUrl);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Thực hiện xóa khỏi Database
                _context.Products.Remove(product);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}