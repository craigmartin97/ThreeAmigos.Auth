﻿using System;
using System.Diagnostics;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ThAmCo.Auth.Data.Account;

namespace ThAmCo.Auth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var env = services.GetRequiredService<IHostingEnvironment>();

                var context = services.GetRequiredService<AccountDbContext>();

                if (env.IsDevelopment()) // if dev, delete the database as its on localdb
                {
                    context.Database.EnsureDeleted(); // delete the database each time and refresh data.
                }

                context.Database.Migrate();
                try
                {
                    AccountDbInitialiser.SeedTestData(context, services).Wait();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + "  " + ex.InnerException);
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogDebug("Seeding test account data failed.");
                }
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseStartup<Startup>();
    }
}
