using Microsoft.AspNetCore.Identity;
using TPI_API.Models;

namespace TPI_API.Seeders;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Bibliotecario", "Ayudante","Admin" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    public static async Task SeedSuperAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var superAdminEmail = "admin@biblioteca.com";
        var superAdminPassword = "Test123.";

        var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);

        if (superAdmin == null)
        {
            var user = new User
            {
                FullName = "Super Admin",
                UserName = superAdminEmail,
                Email = superAdminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, superAdminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRolesAsync(user, new[] { "Bibliotecario", "Admin" });
            }
        }
    }

    public static async Task SeedAyudanteAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var ayudanteEmail = "ayudante@biblioteca.com";
        var ayudantePassword = "Test123.";

        var ayudante = await userManager.FindByEmailAsync(ayudanteEmail);

        if (ayudante == null)
        {
            var user = new User
            {
                FullName = "Ayudante Prueba",
                UserName = ayudanteEmail,
                Email = ayudanteEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, ayudantePassword);

            if (result.Succeeded)
            {
                await userManager.AddToRolesAsync(user, new[] { "Ayudante" });
            }
        }
    }
}
