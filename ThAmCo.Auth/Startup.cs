using IdentityModel.Client;
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

            var b = new ConfigurationBuilder().SetBasePath(hostingEnvironment.ContentRootPath);
            if (hostingEnvironment.IsDevelopment()) // use local db
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
            string authority = env.IsDevelopment() || env.IsStaging() ? "https://localhost:44387" :
                "https://threeamigosauth.azurewebsites.net";

            // add authorizaation polices
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Staff", builder =>
                {
                    builder.RequireClaim("role", "Staff", "Admin"); // only staff and admin
                });
                options.AddPolicy("Admin", builder =>
                {
                    builder.RequireClaim("role", "Admin"); // only admin
                });
            });

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

            // get auth url
            string baseAuthAddress = Configuration["AuthURL"];

            // inject http clients, using named instance
            // authentication injection
            services.AddHttpClient<IAuth, AuthService>("auth", c =>
            {
                c.BaseAddress = new Uri(baseAuthAddress);
                c.DefaultRequestHeaders.Accept.Clear();
                c.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            });

            //setup auth http client
            services.AddSingleton(new ClientCredentialsTokenRequest
            {
                Address = baseAuthAddress + "/connect/token",
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
