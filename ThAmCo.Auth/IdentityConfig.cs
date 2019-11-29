using System;
using System.Collections.Generic;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;

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

                new IdentityResource(name: "roles",
                                     displayName: "ThAmCo Application Roles",
                                     claimTypes: new [] { "role" })
            };
        }

        public static IEnumerable<ApiResource> GetIdentityApis(this IConfiguration configuration)
        {
            return new ApiResource[]
            {
                new ApiResource("thamco_account_api", "ThAmCo Account Management"),

                new ApiResource("staffmanagement_api","Staff management api")
                {
                    UserClaims = { "name","role" }
                }
            };
        }

        public static IEnumerable<Client> GetIdentityClients(this IConfiguration configuration)
        {
            return new[]
            {
                new Client
                {
                    ClientId = "threeamigos_app",
                    ClientName = "Staff management front end",
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
                    ClientId = "staffmanagement_api",
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
                }
            };
        }
    }
}
