using Microsoft.AspNetCore.Identity;
using SmartFood.Core.Entity;

namespace SmartFood.Presentation.Identity
{
    public static class IdentitySeed
    {
        public static async Task SeedRolesAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<Role>>();

            string[] roles =
            {
                "Restaurant",
                "Organization",
                "Admin"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(
                        new Role(role));

                    if (!result.Succeeded)
                    {
                        throw new Exception(
                            $"Failed to create role '{role}': " +
                            string.Join(
                                ", ",
                                result.Errors.Select(
                                    e => e.Description)));
                    }
                }
            }
        }
    }
}