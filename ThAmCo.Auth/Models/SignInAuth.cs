using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ThAmCo.Auth.Models
{
    public class SignInAuth
    {
        public ClaimsPrincipal ClaimsPrincipal { get; set; }

        public AuthenticationProperties AuthenticationProperties { get; set; }
    }
}
