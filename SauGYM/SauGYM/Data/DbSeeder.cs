using Microsoft.AspNetCore.Identity;
using SauGYM.Models;

namespace SauGYM.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            // UserManager ve RoleManager'ı çağırıyoruz
            var userManager = service.GetService<UserManager<AppUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            // 1. ROLLERİ OLUŞTUR (Admin ve Member)
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Member"))
                await roleManager.CreateAsync(new IdentityRole("Member"));

            // 2. ADMIN KULLANCISINI BUL VE ROL ATA
            // Kendi mail adresini buraya tam olarak yazmalısın!
            var adminEmail = "g231210046@sakarya.edu.tr";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser != null)
            {
                // Eğer kullanıcı bulunduysa ve henüz Admin değilse rolü ver
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}