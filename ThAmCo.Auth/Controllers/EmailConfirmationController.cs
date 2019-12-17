using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ThAmCo.Auth.Data.Account;

namespace ThAmCo.Auth.Controllers
{
    public class EmailConfirmationController : Controller
    {
        private UserManager<AppUser> UserManager { get; }

        public EmailConfirmationController(UserManager<AppUser> userManager)
        {
            UserManager = userManager;
        }

        // CRAIG MARTINNN - Confirm Email
        [HttpGet("api/users/confirmemail/{userId}"), AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest();
            }

            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();
            else
            {
                var result = await UserManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return View();
                }
            }

            return BadRequest();
        }
    }
}