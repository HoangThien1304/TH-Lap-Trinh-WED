using BaiTapThucHanh.Data;
using BaiTapThucHanh.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Identity;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CẤU HÌNH CÁC DỊCH VỤ (SERVICES) - PHẢI NẰM TRƯỚC builder.Build()
// =========================================================================

// Cấu hình kết nối với In-Memory Database để test giao diện
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlite("Data Source=BaiTapThucHanh.db"));

// -------------------------------------------------------------------------
// THÊM ĐOẠN NÀY: CẤU HÌNH DỊCH VỤ CỦA ASP.NET CORE IDENTITY
// -------------------------------------------------------------------------
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false; // Tắt bắt buộc xác nhận Email để làm bài tập dễ dàng hơn
    options.Password.RequireDigit = false;          // Không bắt buộc mật khẩu phải có chữ số
    options.Password.RequiredLength = 6;            // Độ dài mật khẩu tối thiểu là 6 ký tự
    options.Password.RequireNonAlphanumeric = false; // Không bắt buộc có ký tự đặc biệt (@, #, $...)
    options.Password.RequireUppercase = false;       // Không bắt buộc phải có chữ viết hoa
    options.Password.RequireLowercase = false;       // Không bắt buộc phải có chữ viết thường
})
.AddRoles<IdentityRole>() // BẮT BUỘC THÊM: Để hệ thống kích hoạt tính năng quản lý phân vai trò (Roles) cho Bài 4
.AddEntityFrameworkStores<AppDbContext>();

// Bắt buộc phải có dịch vụ này thì ứng dụng mới đọc được giao diện Đăng ký/Đăng nhập (Razor Pages) của Identity UI
builder.Services.AddRazorPages();
// -------------------------------------------------------------------------

// Cấu hình Session cho giỏ hàng
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// Thêm các dịch vụ hỗ trợ mô hình MVC (Controller & Views)
builder.Services.AddControllersWithViews();

// Cấu hình mặc định cho tiền tệ và ngày tháng theo chuẩn Việt Nam
var cultureInfo = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

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
app.UseStaticFiles();

// Cho phép ứng dụng đọc và hiển thị hình ảnh từ thư mục "images" ở gốc dự án
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "images")),
    RequestPath = "/images"
});
// ----------------------------------

app.UseRouting();
app.UseSession();

// BẮT BUỘC: Phải kích hoạt Authentication (Xác thực danh tính) TRƯỚC Authorization (Cấp quyền)
app.UseAuthentication();
app.UseAuthorization();

// THÊM DÒNG NÀY: Định tuyến để hệ thống tự động ánh xạ các trang Đăng ký/Đăng nhập của Identity UI
app.MapRazorPages();

// Cấu hình Route cho Areas (Admin) - PHẢI ĐẶT TRƯỚC DEFAULT
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Cấu hình đường tuyến (Route) mặc định điều hướng vào trang chủ HomeController
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// =========================================================================
// THÊM ĐOẠN NÀY: TỰ ĐỘNG CHẠY SEEDER ĐỂ KHỞI TẠO TÀI KHOẢN ADMIN KHI KHỞI ĐỘNG
// =========================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Khởi tạo database schema
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();

        // Chạy hàm tạo Role Admin và User Admin mẫu bất đồng bộ
        await DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception)
    {
        // Ghi nhận lỗi nếu hệ thống không kết nối tới SQL Server thành công lúc khởi động
    }
}
// =========================================================================

app.Run();