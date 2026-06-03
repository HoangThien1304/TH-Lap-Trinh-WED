using Microsoft.AspNetCore.Identity;
using BaiTapThucHanh.Models;
using Microsoft.EntityFrameworkCore;

namespace BaiTapThucHanh.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            // Lấy các dịch vụ quản lý User và Role từ hệ thống
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // 1. Tạo vai trò Admin nếu chưa tồn tại dưới DB
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // 2. Tạo tài khoản Admin mẫu
            string adminEmail = "thientuong316@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                // Tạo User với mật khẩu mẫu là thien@1304
                var createAdmin = await userManager.CreateAsync(newAdmin, "thien@1304");
                if (createAdmin.Succeeded)
                {
                    // Gán quyền Admin cho tài khoản này
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            // 3. Seed Products và Categories
            await SeedProductsAndCategoriesAsync(serviceProvider);
        }

        public static async Task SeedProductsAndCategoriesAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();

            // Nếu đã có dữ liệu thì bỏ qua
            if (await context.Categories.AnyAsync())
                return;

            // Tạo các danh mục
            var categories = new[]
            {
                new Category { Name = "Giày Nike" },
                new Category { Name = "Giày Adidas" },
                new Category { Name = "Giày Jordan" },
                new Category { Name = "Giày Converse" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            // Tạo các sản phẩm mẫu
            var products = new[]
            {
                new Product 
                { 
                    Name = "Nike Air Force 1", 
                    Price = 2500000, 
                    Description = "Giày bóng rổ huyền thoại của Nike, được yêu thích bởi các cầu thủ chuyên nghiệp",
                    CategoryId = categories[0].Id,
                    ImageUrl = null
                },
                new Product 
                { 
                    Name = "Adidas Harden Vol 5", 
                    Price = 2800000, 
                    Description = "Giày bóng rổ cao cấp được thiết kế riêng cho James Harden",
                    CategoryId = categories[1].Id,
                    ImageUrl = null
                },
                new Product 
                { 
                    Name = "Air Jordan 1 Retro High", 
                    Price = 3200000, 
                    Description = "Chiếc giày bóng rổ mang tính biểu tượng nhất từng được tạo ra",
                    CategoryId = categories[2].Id,
                    ImageUrl = null
                },
                new Product 
                { 
                    Name = "Converse Chuck Taylor All Star Pro", 
                    Price = 1800000, 
                    Description = "Giày bóng rổ kinh điển với thiết kế đơn giản và thoải mái",
                    CategoryId = categories[3].Id,
                    ImageUrl = null
                },
                new Product 
                { 
                    Name = "Nike LeBron Witness 6", 
                    Price = 2600000, 
                    Description = "Giày bóng rổ hiệu suất cao được sử dụng bởi LeBron James",
                    CategoryId = categories[0].Id,
                    ImageUrl = null
                },
                new Product 
                { 
                    Name = "Adidas Dame 8", 
                    Price = 2700000, 
                    Description = "Giày bóng rổ được thiết kế cho Damian Lillard, tập trung vào tốc độ",
                    CategoryId = categories[1].Id,
                    ImageUrl = null
                },
                new Product 
                { 
                    Name = "Air Jordan XXXVII", 
                    Price = 3500000, 
                    Description = "Phiên bản mới nhất của dòng giày Jordan nổi tiếng",
                    CategoryId = categories[2].Id,
                    ImageUrl = null
                },
                new Product 
                { 
                    Name = "Nike Kyrie 8", 
                    Price = 2900000, 
                    Description = "Giày bóng rổ dành cho những cầu thủ nhanh nhẹn và kỹ thuật cao",
                    CategoryId = categories[0].Id,
                    ImageUrl = null
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}