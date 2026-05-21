using BaiTapThucHanh.Data;
using BaiTapThucHanh.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders; // Thư viện bắt buộc để dùng PhysicalFileProvider
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CẤU HÌNH CÁC DỊCH VỤ (SERVICES) - PHẢI NẰM TRƯỚC builder.Build()
// =========================================================================

// Cấu hình kết nối SQL Server thông qua AppDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Thêm các dịch vụ hỗ trợ mô hình MVC (Controller & Views)
builder.Services.AddControllersWithViews();

var app = builder.Build();

// =========================================================================
// 2. CẤU HÌNH PIPELINE / MIDDLEWARE - PHẢI NẰM SAU var app = builder.Build()
// =========================================================================

// Cấu hình xử lý lỗi môi trường Production
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// --- CẤU HÌNH PHỤC VỤ FILE TĨNH ---
// 1. Dòng mặc định để hệ thống tự động đọc file tĩnh trong thư mục wwwroot
app.UseStaticFiles();

// 2. CẤU HÌNH THÊM: Cho phép ứng dụng đọc và hiển thị hình ảnh từ thư mục "images" ở gốc dự án
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "images")), // Thay builder thành app cho đúng ngữ cảnh sau Build
    RequestPath = "/images"
});
// ----------------------------------

app.UseRouting();
app.UseAuthorization();

// Cấu hình đường tuyến (Route) mặc định điều hướng vào trang chủ HomeController
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();