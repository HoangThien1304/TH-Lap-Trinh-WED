using Microsoft.EntityFrameworkCore;
using BaiTapThucHanh.Models; 

namespace BaiTapThucHanh.Data 
{
    public class AppDbContext : DbContext
    {
        // Constructor đã đồng bộ với tên AppDbContext
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Bảng quản lý sản phẩm
        public DbSet<Product> Products { get; set; }

        // FIX: Sửa từ <Product> thành <Category> và đổi tên thuộc tính thành Categories (số nhiều) cho đúng chuẩn
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình định dạng số thập phân cho thuộc tính Price của Product
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");
        }
    }
}