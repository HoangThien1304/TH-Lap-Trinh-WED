using BaiTapThucHanh.Models;
using BaiTapThucHanh.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization; // THÊM THƯ VIỆN NÀY ĐỂ SỬ DỤNG BỘ LỌC PHÂN QUYỀN AUTHORIZE
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaiTapThucHanh.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================================
        // ĐỌC DANH SÁCH SẢN PHẨM (INDEX)
        // Cho phép mọi đối tượng (kể cả chưa đăng nhập) được xem công khai
        // ==========================================
        [AllowAnonymous]
        public IActionResult Index()
        {
            var products = _context.Products.Include(p => p.Category).ToList();
            return View(products);
        }

        // ==========================================
        // XEM CHI TIẾT SẢN PHẨM (DISPLAY)
        // Cho phép mọi đối tượng được vào xem chi tiết cấu trúc ảnh
        // ==========================================
        [AllowAnonymous]
        public IActionResult Display(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // ==========================================
        // 1. THÊM SẢN PHẨM & TỰ ĐỘNG SAO CHÉP ẢNH SANG BẢNG PHỤ (ADD)
        // CHỈ CHO PHÉP TÀI KHOẢN CÓ QUYỀN ADMIN TRUY CẬP
        // ==========================================
        [Authorize(Roles = "Admin")]
        public IActionResult Add()
        {
            var categories = _context.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Add(Product product, IFormFileCollection subImages)
        {
            if (ModelState.IsValid)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // A. Xử lý lưu Ảnh đại diện chính của Product
                if (product.ImageFile != null)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + product.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        product.ImageFile.CopyTo(fileStream);
                    }
                    product.ImageUrl = uniqueFileName;
                }

                // BƯỚC QUAN TRỌNG: Lưu sản phẩm trước để hệ thống sinh ra bản ghi 
                // và cấp mã số định danh Product.Id tự tăng từ SQL Server
                _context.Products.Add(product);
                _context.SaveChanges();

                // B. Duyệt danh sách ảnh đính kèm để sao chép dữ liệu ImageUrl và ProductId
                if (subImages != null && subImages.Count > 0)
                {
                    foreach (var file in subImages)
                    {
                        if (file != null && file.Length > 0)
                        {
                            string uniqueSubName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string subPath = Path.Combine(uploadsFolder, uniqueSubName);

                            using (var fileStream = new FileStream(subPath, FileMode.Create))
                            {
                                file.CopyTo(fileStream);
                            }

                            // === ĐỒNG BỘ SAO CHÉP SANG PRODUCT IMAGE ===
                            var productImage = new ProductImage
                            {
                                ImageUrl = uniqueSubName, // Lưu tên file (tương tự như ImageUrl bên Product)
                                ProductId = product.Id    // Sao chép chính xác ID của sản phẩm vừa tạo ở trên
                            };
                            _context.ProductImages.Add(productImage);
                        }
                    }
                    _context.SaveChanges(); // Lưu lại toàn bộ danh sách ảnh phụ vào cơ sở dữ liệu
                }

                return RedirectToAction("Index");
            }

            var categories = _context.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        // ==========================================
        // 2. CẬP NHẬT SẢN PHẨM & SAO CHÉP THÊM ẢNH PHỤ (UPDATE)
        // CHỈ CHO PHÉP TÀI KHOẢN CÓ QUYỀN ADMIN TRUY CẬP
        // ==========================================
        [Authorize(Roles = "Admin")]
        public ActionResult Update(int id)
        {
            var product = _context.Products.Include(p => p.ProductImages).FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = _context.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(Product product, IFormFileCollection subImages)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = _context.Products.AsNoTracking().FirstOrDefault(p => p.Id == product.Id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                string uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "images");

                // A. Cập nhật Ảnh đại diện chính nếu có chọn file mới
                if (product.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        string oldFilePath = Path.Combine(uploadsFolder, existingProduct.ImageUrl);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

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
                    product.ImageUrl = existingProduct.ImageUrl;
                }

                // Thực hiện cập nhật thông tin sản phẩm chính trước
                _context.Products.Update(product);
                _context.SaveChanges();

                // B. Nếu người dùng tải thêm ảnh phụ mới khi update dữ liệu
                if (subImages != null && subImages.Count > 0)
                {
                    foreach (var file in subImages)
                    {
                        if (file != null && file.Length > 0)
                        {
                            string uniqueSubName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string subPath = Path.Combine(uploadsFolder, uniqueSubName);

                            using (var fileStream = new FileStream(subPath, FileMode.Create))
                            {
                                file.CopyTo(fileStream);
                            }

                            // === ĐỒNG BỘ SAO CHÉP SANG PRODUCT IMAGE KHI UPDATE ===
                            var productImage = new ProductImage
                            {
                                ImageUrl = uniqueSubName, // Đồng bộ cấu trúc tên file hình ảnh phụ
                                ProductId = product.Id    // Gắn chính xác Id của sản phẩm đang chỉnh sửa
                            };
                            _context.ProductImages.Add(productImage);
                        }
                    }
                    _context.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            var categoriesList = _context.Categories.ToList();
            ViewBag.Categories = new SelectList(categoriesList, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // ==========================================
        // XÓA LẺ TỪNG ẢNH PHỤ TRONG BỘ SƯU TẬP
        // CHỈ CHO PHÉP TÀI KHOẢN CÓ QUYỀN ADMIN THỰC HIỆN
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteImage(int imageId)
        {
            var img = _context.ProductImages.FirstOrDefault(p => p.Id == imageId);
            if (img != null)
            {
                string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "images", img.ImageUrl);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.ProductImages.Remove(img);
                _context.SaveChanges();

                return Json(new { success = true, message = "Xóa ảnh phụ thành công!" });
            }
            return Json(new { success = false, message = "Không tìm thấy ảnh cần xóa." });
        }

        // ==========================================
        // 3. ĐỒNG BỘ XÓA SẢN PHẨM & TOÀN BỘ BỘ ẢNH (DELETE)
        // CHỈ CHO PHÉP TÀI KHOẢN CÓ QUYỀN ADMIN TRUY CẬP
        // ==========================================
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _context.Products.Include(p => p.ProductImages).FirstOrDefault(p => p.Id == id);

            if (product != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "images");

                // Duyệt vòng lặp dọn sạch toàn bộ các file ảnh phụ trên ổ đĩa
                if (product.ProductImages != null && product.ProductImages.Any())
                {
                    foreach (var img in product.ProductImages)
                    {
                        string subFilePath = Path.Combine(uploadsFolder, img.ImageUrl);
                        if (System.IO.File.Exists(subFilePath))
                        {
                            System.IO.File.Delete(subFilePath);
                        }
                    }
                }

                // Xóa file ảnh đại diện chính của sản phẩm
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    string mainFilePath = Path.Combine(uploadsFolder, product.ImageUrl);
                    if (System.IO.File.Exists(mainFilePath))
                    {
                        System.IO.File.Delete(mainFilePath);
                    }
                }

                _context.Products.Remove(product);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}