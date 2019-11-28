using System;
using Microsoft.AspNetCore.Identity;

namespace ThAmCo.Auth.Data.Account
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
