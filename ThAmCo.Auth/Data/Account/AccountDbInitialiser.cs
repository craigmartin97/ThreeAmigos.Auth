using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ThAmCo.Auth.Data.Account
{
    static public class AccountDbInitialiser
    {
        public static async Task SeedTestData(AccountDbContext context,
                                              IServiceProvider services)
        {
            // exit early if any existing data is present
            if (context.Users.Any())
            {
                return;
            }

            var userManager = services.GetRequiredService<UserManager<AppUser>>();

            AppUser[] users = {
                new AppUser { UserName = "admin@example.com", Email = "admin@example.com", FullName = "Example Admin User" },
                new AppUser { UserName = "staff@example.com", Email = "staff@example.com", FullName = "Example Staff User" },
                new AppUser { UserName = "bob@example.com", Email = "bob@example.com", FullName = "Robert 'Bobby' Robertson" },
                new AppUser { UserName = "betty@example.com", Email = "betty@example.com", FullName = "Bethany 'Betty' Roberts"  }
            };
            foreach (var user in users)
            {
                await userManager.CreateAsync(user, "Password1_");
                // auto confirm email addresses for test users
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                await userManager.ConfirmEmailAsync(user, token);
            }

            await userManager.AddToRoleAsync(users[0], "Admin");
            await userManager.AddToRoleAsync(users[1], "Staff");
            await userManager.AddToRoleAsync(users[2], "Customer");
            await userManager.AddToRoleAsync(users[3], "Customer");
        }
    }
}
