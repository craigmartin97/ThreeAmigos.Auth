﻿using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ThAmCo.Auth
{
    public static class IdentityConfigurationExtensions
    {
        public static IEnumerable<IdentityResource> GetIdentityResources(this IConfiguration configuration)
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),

                new IdentityResources.Profile(),

                new IdentityResource("roles", new[]{ "role"})
            };
        }

        public static IEnumerable<ApiResource> GetIdentityApis(this IConfiguration configuration)
        {
            return new ApiResource[]
            {
                new ApiResource("thamco_account_api", "ThAmCo Account Management"),

                new ApiResource("staff_api","Staff management api")
                {
                    UserClaims = { "name","role" } // maybe add data here? should it be ro;es?
                }
            };
        }

        public static IEnumerable<Client> GetIdentityClients(this IConfiguration configuration)
        {
            /**
             * Craig Martin Q5031372 Added clients to access oauth server
             */
            return new[]
            {
                new Client
                {
                    ClientId = "staff_api",
                    ClientName = "Staff management api",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes =
                    {
                        "thamco_account_api"
                    },
                    RequireConsent = false
                },
                new Client
                {
                    ClientId = "threeamigos_app",
                    ClientName = "Staff management front end",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "thamco_account_api",
                        "profile",
                        "staff_api",
                        "roles"
                    },
                    RequireConsent = false,
                    AllowOfflineAccess = true // added to test refresh token 23/12/19
                },
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // where to redirect to after login
                    RedirectUris = { "https://localhost:44373/signin-oidc" },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "https://localhost:44373/signout-callback-oidc" },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "roles",
                        "staff_api" // <---- Add staff api as scope
                    },
                    RequireConsent = false,
                    AllowOfflineAccess = true
                }
                ,new Client
                {
                    ClientId = "livemvc",
                    ClientName = "MVC Client",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // where to redirect to after login
                    RedirectUris = { "https://threeamigosapp.azurewebsites.net/signin-oidc" },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "https://threeamigosapp.azurewebsites.net/signout-callback-oidc" },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "roles",
                        "staff_api" // <---- Add staff api as scope
                    },
                    RequireConsent = false,
                    AllowOfflineAccess = true
                }
            };
        }
    }
}
