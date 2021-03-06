﻿using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel.Tokens.Jwt;
using ThAmCo.Auth.Data.Account;
using ThAmCo.Auth.Service;

namespace ThAmCo.Auth
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        private IHostingEnvironment env;

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            env = hostingEnvironment;
            Configuration = configuration;

            /**
             * This is called UserSecrets. 
             * A folder (f18708ad-8894-465a-994c-3effeb77dc97) with a secret.json file must 
             * be on the users development machine in order to get an email and password from.
             * Otherwise, the reset email and confirmation email functionality won't work.
             * On Azure, the email and password are added as configration settings.
             * UserSecrets are used to avoid adding personal details into source controler and its the Microsoft recommneded way.
             */
            var b = new ConfigurationBuilder().SetBasePath(hostingEnvironment.ContentRootPath);
            if (hostingEnvironment.IsDevelopment())
            {
                b.AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json",
                    optional: false, reloadOnChange: true).AddUserSecrets<Startup>();
                Configuration = b.Build();
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // configure EF context to use for storing Identity account data
            services.AddDbContext<AccountDbContext>(options => options.UseSqlServer(
                Configuration.GetConnectionString("AccountConnection"),
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", "account")
            ));

            // configure Identity account management
            services.AddIdentity<AppUser, AppRole>()
                    .AddEntityFrameworkStores<AccountDbContext>()
                    .AddDefaultTokenProviders();

            // add bespoke factory to translate our AppUser into claims
            services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppClaimsPrincipalFactory>();

            // CRAIG MARTIN
            // Add singleton configuration to access username and password for smtp server
            services.AddSingleton(Configuration);

            // configure Identity security options
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredUniqueChars = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;

                // Sign-in settings
                options.SignIn.RequireConfirmedEmail = true; //false;
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // if development or staging then use localhost else use live server.
            //string authority = env.IsDevelopment() || env.IsStaging() ? "https://localhost:44387" :
            //    "https://threeamigosauth.azurewebsites.net";
            string authority = Configuration["AuthURL"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                    .AddJwtBearer("thamco_account_api", options =>
                    {
                        options.Audience = "thamco_account_api";
                        options.Authority = authority;
                        options.RequireHttpsMetadata = false;
                    })
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // configure IdentityServer (provides OpenId Connect and OAuth2)
            services.AddIdentityServer(options =>
                    {
                        options.IssuerUri = authority;
                        options.PublicOrigin = authority;
                    })
                    .AddInMemoryIdentityResources(Configuration.GetIdentityResources())
                    .AddInMemoryApiResources(Configuration.GetIdentityApis())
                    .AddInMemoryClients(Configuration.GetIdentityClients())
                    .AddAspNetIdentity<AppUser>()
                    .AddDeveloperSigningCredential();
            // TODO: developer signing cert above should be replaced with a real one
            // TODO: should use AddOperationalStore to persist tokens between app executions

            // inject http clients, using named instance
            // authentication injection
            services.AddHttpClient<IAuth, AuthService>("auth", c =>
            {
                c.BaseAddress = new Uri(authority);
                c.DefaultRequestHeaders.Accept.Clear();
                c.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            });

            //setup auth http client
            services.AddSingleton(new ClientCredentialsTokenRequest
            {
                Address = authority + "/connect/token",
                ClientId = "threeamigos_app",
                ClientSecret = "secret"
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

            var forwardOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                RequireHeaderSymmetry = false
            };
            forwardOptions.KnownNetworks.Clear();
            forwardOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardOptions);

            // use IdentityServer middleware during HTTP requests
            app.UseIdentityServer();

            app.UseMvc();
        }
    }
}
