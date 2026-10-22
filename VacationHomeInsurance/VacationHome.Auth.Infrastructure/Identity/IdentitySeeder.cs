using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VacationHome.Auth.Domain.Constants;
using VacationHome.Auth.Domain.Entities;


namespace VacationHome.Auth.Infrastructure.Identity
{


    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();


            var roleManager =
                scope.ServiceProvider
                    .GetRequiredService<RoleManager<IdentityRole>>();


            var userManager =
                scope.ServiceProvider
                    .GetRequiredService<UserManager<ApplicationUser>>();


            await SeedRolesAsync(roleManager);


            await SeedAdminAsync(userManager);
        }


        private static async Task SeedRolesAsync(
            RoleManager<IdentityRole> roleManager)
        {
            string[] roles =
            {
            Roles.Admin,
            Roles.Customer,
            Roles.ClaimsManager
        };


            foreach (var role in roles)
            {
                var exists =
                    await roleManager.RoleExistsAsync(role);


                if (!exists)
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }
        }


        private static async Task SeedAdminAsync(
            UserManager<ApplicationUser> userManager)
        {
            const string adminEmail =
                "admin@vacationhome.com";


            const string adminPassword =
                "Admin@123";


            var admin =
                await userManager.FindByEmailAsync(
                    adminEmail);


            if (admin is not null)
            {
                return;
            }


            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true
            };


            var result =
                await userManager.CreateAsync(
                    admin,
                    adminPassword);


            if (!result.Succeeded)
            {
                throw new Exception(
                    $"Failed to create admin user: {string.Join(", ", result.Errors.Select(x => x.Description))}");
            }


            await userManager.AddToRoleAsync(
                admin,
                Roles.Admin);
        }
    }
}
