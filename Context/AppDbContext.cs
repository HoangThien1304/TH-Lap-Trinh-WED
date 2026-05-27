using Microsoft.EntityFrameworkCore;
using BaiTapThucHanh.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // THÊM THƯ VIỆN NÀY ĐỂ DÙNG IDENTITYDBCONTEXT
using Microsoft.AspNetCore.Identity; // Thư viện hỗ trợ các thực thể Identity mặc định

namespace BaiTapThucHanh.Data
{
    // ĐỔI KẾ THỪA: Thay DbContext bằng IdentityDbContext để tự động tích hợp các bảng User, Role, Token...
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        // Cập nhật lại DbContextOptions theo kiểu thực thể của AppDbContext
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // BẮT BUỘC: Phải giữ nguyên dòng base.OnModelCreating này khi dùng Identity 
            // để hệ thống tự động ánh xạ các cấu hình bảng tài khoản (AspNetUsers, AspNetRoles...) vào SQL Server
            base.OnModelCreating(modelBuilder);

            // Cấu hình định dạng số thập phân cho thuộc tính Price của Product
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");
        }
    }
}