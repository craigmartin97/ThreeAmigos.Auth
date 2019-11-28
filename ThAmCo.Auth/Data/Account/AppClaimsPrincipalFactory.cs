using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ThAmCo.Auth.Data.Account
{
    public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, AppRole>
    {
        public AppClaimsPrincipalFactory(UserManager<AppUser> userManager,
                                         RoleManager<AppRole> roleManager,
                                         IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        public async override Task<ClaimsPrincipal> CreateAsync(AppUser user)
        {
            var principal = await base.CreateAsync(user);
            var claimsIdentity = principal.Identity as ClaimsIdentity;
            claimsIdentity.AddClaims(new[] {
                new Claim(JwtClaimTypes.Name, user.FullName),
            });

            return principal;
        }
    }
}
