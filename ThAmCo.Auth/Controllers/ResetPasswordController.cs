using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ThAmCo.Auth.Data.Account;
using ThAmCo.Auth.Models;

namespace ThAmCo.Auth.Controllers
{
    public class ResetPasswordController : Controller
    {
        private UserManager<AppUser> UserManager { get; }

        public ResetPasswordController(UserManager<AppUser> userManager)
        {
            UserManager = userManager;
        }

        [HttpGet("api/ResetPassword"), AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
                ModelState.AddModelError("", "Invalid password reset token");

            return View();
        }

        [HttpPost("api/ResetPassword"), AllowAnonymous]
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