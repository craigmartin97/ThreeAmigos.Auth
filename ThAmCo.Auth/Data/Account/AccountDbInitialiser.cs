using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ThAmCo.Auth.Helpers;

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

            RandomWordsGenerator fn = new RandomWordsGenerator("https://raw.githubusercontent.com/dominictarr/random-name/master/first-names.txt");
            RandomWordsGenerator ln = new RandomWordsGenerator("https://raw.githubusercontent.com/arineng/arincli/master/lib/last-names.txt");

            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            Random random = new Random();
            string[] levels = new string[]
            {
                "Admin",
                "Staff",
                "Customer"
            };

            for (int i = 0; i < 500; i++)
            {
                string firstName = fn.GetWord();
                string lastName = ln.GetWord();
                AppUser user = new AppUser()
                {
                    Email = firstName + lastName + "@example.com",
                    UserName = firstName + lastName + "@example.com",
                    FullName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(firstName)
                    + " " +
                    System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lastName)
                };

                await userManager.CreateAsync(user, ".Password123");
                // auto confirm email addresses for test users
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                await userManager.ConfirmEmailAsync(user, token);


                await userManager.AddToRoleAsync(user, levels[random.Next(levels.Length)]);
            }


            AppUser[] users = {
                    new AppUser { UserName = "admin@example.com", Email = "admin@example.com", FullName = "Example Admin User" },
                    new AppUser { UserName = "staff@example.com", Email = "staff@example.com", FullName = "Example Staff User" },
                    new AppUser { UserName = "bob@example.com", Email = "bob@example.com", FullName = "Robert 'Bobby' Robertson" },
                    new AppUser { UserName = "betty@example.com", Email = "betty@example.com", FullName = "Bethany 'Betty' Roberts"  }
                };
            foreach (var user in users)
            {
                await userManager.CreateAsync(user, ".Password123");
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
