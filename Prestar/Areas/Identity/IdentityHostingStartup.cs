using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prestar.Data;

[assembly: HostingStartup(typeof(Prestar.Areas.Identity.IdentityHostingStartup))]
namespace Prestar.Areas.Identity
{
    /// <summary>
    /// Identity Hosting Startup.
    /// Inherits IHostingStartup
    /// <see cref="IHostingStartup"/>
    /// </summary>
    public class IdentityHostingStartup : IHostingStartup
    {
        /// <summary>
        /// Configures services
        /// </summary>
        /// <param name="builder">
        /// Web host builder
        /// <see cref="IWebHostBuilder"/>
        /// </param>
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}