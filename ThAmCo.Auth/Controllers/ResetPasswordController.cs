using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using ThAmCo.Auth.Data.Account;
using ThAmCo.Auth.Models;

namespace ThAmCo.Auth.Controllers
{
    public class ResetPasswordController : Controller
    {
        private UserManager<AppUser> UserManager { get; }

        private readonly IConfiguration configuration;

        public ResetPasswordController(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            UserManager = userManager;
            this.configuration = configuration;
        }

        [HttpPost("api/sendresetrequest"), AllowAnonymous] //api/users/resetpassword
        public async Task<IActionResult> SendResetPasswordToken([FromBody] string email)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                var user = await UserManager.FindByEmailAsync(email);
                if (user != null)
                {
                    string token = await UserManager.GeneratePasswordResetTokenAsync(user);
                    string confirmationLink = Url.Action("ResetPassword", "ResetPassword", new
                    {
                        token = token,
                        email = user.Email
                    },
                    Request.Scheme);

                    // Send token
                    EmailSender emailSender = new EmailSender();
                    emailSender.SendEmail(user, configuration, confirmationLink, "Three Amigos -- Change Password");

                    return Ok();
                }
                return NotFound();
            }

            return BadRequest();
        }

        [HttpGet("api/resetpassword"), AllowAnonymous] //ResetPassword
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
                ModelState.AddModelError("", "Invalid password reset token");

            return View();
        }

        [HttpPost("api/resetpassword"), AllowAnonymous] //ResetPassword
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(viewModel.Email);
                if (user != null)
                {
                    var result = await UserManager.ResetPasswordAsync(user, viewModel.Token, viewModel.Password);
                    if (result.Succeeded)
                    {
                        return View("ResetPasswordConfirmation");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(viewModel);
                }
                return View("ResetPasswordConfirmation");
            }

            return View(viewModel);
        }
    }
}