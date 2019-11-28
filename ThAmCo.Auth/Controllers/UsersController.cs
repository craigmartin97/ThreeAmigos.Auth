using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ThAmCo.Auth.Data.Account;
using ThAmCo.Auth.Models;

namespace ThAmCo.Auth.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "thamco_account_api")]
    public class UsersController : ControllerBase
    {
        private UserManager<AppUser> UserManager { get; }

        public UsersController(UserManager<AppUser> userManager)
        {
            UserManager = userManager;
        }

        [HttpGet("api/users")]
        public async Task<IActionResult> GetUsers([FromQuery] string role = null)
        {
            var users = string.IsNullOrEmpty(role) ? UserManager.Users.ToList()
                                                   : await UserManager.GetUsersInRoleAsync(role);
            var dto = users.Select(u => new UserSummaryGetDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.FullName
            });
            return Ok(dto);
        }

        [HttpGet("api/users/{userId}")]
        public async Task<IActionResult> GetUser([FromRoute] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }

            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await UserManager.GetRolesAsync(user);

            var dto = new UserGetDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Roles = roles
            };

            return Ok(dto);
        }

        [HttpPost("api/users")]
        public async Task<IActionResult> AddUser([FromBody] UserPutDto newUser)
        {
            if (newUser == null)
            {
                return BadRequest();
            }

            var user = new AppUser
            {
                Email = newUser.Email,
                FullName = newUser.FullName,
                UserName = newUser.Email
            };

            var result = await UserManager.CreateAsync(user, newUser.Password);
            if (!result.Succeeded)
            {
                return BadRequest();
            }

            user = await UserManager.FindByEmailAsync(newUser.Email);
            await UserManager.AddToRolesAsync(user, newUser.Roles);

            var roles = await UserManager.GetRolesAsync(user);

            var dto = new UserGetDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Roles = roles
            };

            return Ok(dto);
        }

        [HttpDelete("api/users/{userId}")]
        public async Task<IActionResult> RemoveUser([FromRoute] string userId = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }

            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await UserManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok();
        }

        [HttpPut("api/users/{userId}")]
        public async Task<IActionResult> UpdateUser([FromRoute] string userId,
                                                    [FromBody] UserPutDto updatedUser)
        {
            if (string.IsNullOrEmpty(userId) || updatedUser == null)
            {
                return BadRequest();
            }

            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.Email = updatedUser.Email ?? user.Email;
            user.FullName = updatedUser.FullName ?? user.FullName;

            await UserManager.UpdateAsync(user);

            if (!string.IsNullOrEmpty(updatedUser.Password))
            {
                await UserManager.RemovePasswordAsync(user);
                await UserManager.AddPasswordAsync(user, updatedUser.Password);
            }

            var roles = await UserManager.GetRolesAsync(user);

            var rolesToAdd = updatedUser.Roles.Where(r => !roles.Contains(r));
            await UserManager.AddToRolesAsync(user, rolesToAdd);

            var rolesToRemove = roles.Where(r => !updatedUser.Roles.Contains(r));
            await UserManager.RemoveFromRolesAsync(user, rolesToRemove);

            roles = await UserManager.GetRolesAsync(user);

            var dto = new UserGetDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Roles = roles
            };

            return Ok(dto);
        }
    }
}
