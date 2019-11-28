using System;
using Microsoft.AspNetCore.Identity;

namespace ThAmCo.Auth.Data.Account
{
    public class AppRole : IdentityRole
    {
        public string Descriptor { get; set; }
    }
}
