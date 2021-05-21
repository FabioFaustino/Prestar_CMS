using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore;

namespace Prestar
{
    /// <summary>
    /// Application Main
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args)      
               .UseUrls("http://0.0.0.0:" + Environment.GetEnvironmentVariable("PORT"))
               
               .UseIISIntegration()
               .Build();

            using (var scope = host.Services.CreateScope())
            {
                // extra configuration
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)                
                .UseStartup<Startup>();
    }

}

