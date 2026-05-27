using Microsoft.AspNetCore.Identity;

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
            string adminEmail = "admin@huy88.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                // Tạo User với mật khẩu mẫu là Admin@123
                var createAdmin = await userManager.CreateAsync(newAdmin, "Admin@123");
                if (createAdmin.Succeeded)
                {
                    // Gán quyền Admin cho tài khoản này
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }
    }
}