using Microsoft.AspNetCore.Identity;

namespace NorthwindApp.Data
{
    public static class SeedData
    {
        public static async Task Initialize(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // ===== ROLES =====
            string[] roles = { "Admin", "Employee", "Client" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ===== USUARIO ADMIN =====
            var adminEmail = "admin@northwind.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // ===== USUARIO EMPLOYEE =====
            var employeeEmail = "employee@northwind.com";
            if (await userManager.FindByEmailAsync(employeeEmail) == null)
            {
                var employee = new IdentityUser
                {
                    UserName = employeeEmail,
                    Email = employeeEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(employee, "Employee123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(employee, "Employee");
            }

            // ===== USUARIO CLIENT =====
            var clientEmail = "cliente@northwind.com";
            if (await userManager.FindByEmailAsync(clientEmail) == null)
            {
                var client = new IdentityUser
                {
                    UserName = clientEmail,
                    Email = clientEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(client, "Cliente123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(client, "Client");
            }
        }
    }
}