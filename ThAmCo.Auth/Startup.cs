using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel.Tokens.Jwt;
using ThAmCo.Auth.Data.Account;

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
                    optional: false, reloadOnChange: true);
                Configuration = b.Build();
            }
            else if (hostingEnvironment.IsStaging()) // if staging, so debuging live db
            {
                b.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddUserSecrets<Startup>();
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

            var s = Configuration.GetConnectionString("AccountConnection");

            // configure Identity account management
            services.AddIdentity<AppUser, AppRole>()
                    .AddEntityFrameworkStores<AccountDbContext>()
                    .AddDefaultTokenProviders();

            // add bespoke factory to translate our AppUser into claims
            services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppClaimsPrincipalFactory>();

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
                options.SignIn.RequireConfirmedEmail = false;
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // if development or staging then use localhost else use live server.
            string authority = env.IsDevelopment() || env.IsStaging() ? "https://localhost:44387" :
                "https://threeamigosauth.azurewebsites.net";

            services.AddAuthentication()
                    .AddJwtBearer("thamco_account_api", options =>
                    {
                        options.Audience = "thamco_account_api";
                        options.Authority = authority;
                        options.RequireHttpsMetadata = false;
                    });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // configure IdentityServer (provides OpenId Connect and OAuth2)
            services.AddIdentityServer()
                    .AddInMemoryIdentityResources(Configuration.GetIdentityResources())
                    .AddInMemoryApiResources(Configuration.GetIdentityApis())
                    .AddInMemoryClients(Configuration.GetIdentityClients())
                    .AddAspNetIdentity<AppUser>()
                    .AddDeveloperSigningCredential();
            // TODO: developer signing cert above should be replaced with a real one
            // TODO: should use AddOperationalStore to persist tokens between app executions
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

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

            // use IdentityServer middleware during HTTP requests
            app.UseIdentityServer();

            app.UseMvc();
        }
    }
}
