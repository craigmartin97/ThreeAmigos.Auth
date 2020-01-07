using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using ThAmCo.Auth.Models;

namespace ThAmCo.Auth.Service
{
    public class AuthService : IAuth
    {
        private readonly HttpClient _httpClient;

        private readonly ClientCredentialsTokenRequest _clientCredentials;

        public AuthService(HttpClient httpClient, ClientCredentialsTokenRequest tokenRequest)
        {
            _httpClient = httpClient;
            _clientCredentials = tokenRequest;
        }

        [AllowAnonymous]
        public async Task<SignInAuth> RequestToken(LoginViewModel user)
        {
            DiscoveryResponse disco = await _httpClient.GetDiscoveryDocumentAsync();
            TokenResponse passwordToken = await _httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = _clientCredentials.ClientId,
                ClientSecret = _clientCredentials.ClientSecret,
                Scope = "openid thamco_account_api roles staff_api",

                UserName = user.Email,
                Password = user.Password
            });

            if (passwordToken.IsError)
            {
                throw new HttpRequestException("Unable to sign in with the details provided. Please check your email and password and try again.");
            }

            UserInfoResponse response = await _httpClient.GetUserInfoAsync(new UserInfoRequest
            {
                Address = disco.UserInfoEndpoint,
                Token = passwordToken.AccessToken
            });

            if(response == null)
            {
                throw new NullReferenceException("Unable to get user details. Please try again.");
            }

            ClaimsIdentity claimIdentity = new ClaimsIdentity(response.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimIdentity);

            AuthenticationToken[] tokenStore = new AuthenticationToken[]
            {
                new AuthenticationToken{ Name = "access_token", Value=passwordToken.AccessToken }
            };

            AuthenticationProperties authProp = new AuthenticationProperties();
            authProp.StoreTokens(tokenStore);
            authProp.IsPersistent = user.RememberMe; // retain cookies

            return new SignInAuth()
            {
                AuthenticationProperties = authProp,
                ClaimsPrincipal = claimsPrincipal
            };
        }
    }
}
