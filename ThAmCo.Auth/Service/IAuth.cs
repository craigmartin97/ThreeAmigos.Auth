using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThAmCo.Auth.Models;

namespace ThAmCo.Auth.Service
{
    public interface IAuth
    {
        /// <summary>
        /// Request a password access token
        /// </summary>
        /// <param name="user">The user object to retrive a token for</param>
        /// <returns></returns>
        Task<SignInAuth> RequestToken(LoginViewModel user);
    }
}
