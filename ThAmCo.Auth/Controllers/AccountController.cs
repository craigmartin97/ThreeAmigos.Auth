using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThAmCo.Auth.Models;
using ThAmCo.Auth.Service;

namespace ThAmCo.Auth.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuth _auth;
        public AccountController(IAuth auth)
        {
            _auth = auth;
        }

        [HttpGet("Account/Login"), AllowAnonymous]
        public IActionResult Login([FromQuery] string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// Perform the logic to log the user into there account and retrieve an access token
        /// </summary>
        /// <param name="user">Users entered data on form</param>
        /// <param name="returnUrl"></param>
        /// <returns>Redirect to the a new view or redisplay the same view with an error message</returns>
        [HttpPost("Account/Login"), AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginViewModel user,
                                               [FromQuery] string returnUrl)
        {
            if (ModelState.IsValid)
            {
                SignInAuth response = await _auth.RequestToken(user);

                // set remember me
                response.AuthenticationProperties.IsPersistent = user.RememberMe;
                await HttpContext.SignInAsync
                      (CookieAuthenticationDefaults.AuthenticationScheme,
                        response.ClaimsPrincipal,
                        response.AuthenticationProperties
                      );
                return Redirect(returnUrl ?? "/");

            }
            // add return url back in!!!
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpGet("Account/Login/Admin"), Authorize]
        public IActionResult AdminOnly()
        {
            return View();
        }
    }
}