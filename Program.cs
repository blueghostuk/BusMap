using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;

namespace BusMap
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var azAppConfigSettings = config.Build();
                    var azAppConfigConnection = azAppConfigSettings["AppConfig"];
                    if (!string.IsNullOrEmpty(azAppConfigConnection))
                    {
                        // Use the connection string if it is available.
                        config.AddAzureAppConfiguration(options =>
                        {
                            options.Connect(azAppConfigConnection)
                            .ConfigureRefresh(refresh =>
                            {
                                // All configuration values will be refreshed if the sentinel key changes.
                                refresh.Register("TestApp:Settings:Sentinel", refreshAll: true);
                            });
                        });
                    }
                    else if (Uri.TryCreate(azAppConfigSettings["Endpoints:AppConfig"], UriKind.Absolute, out var endpoint))
                    {
                        // Use Azure Active Directory authentication.
                        // The identity of this app should be assigned 'App Configuration Data Reader' or 'App Configuration Data Owner' role in App Configuration.
                        // For more information, please visit https://aka.ms/vs/azure-app-configuration/concept-enable-rbac
                        config.AddAzureAppConfiguration(options =>
                        {
                            options.Connect(endpoint, new DefaultAzureCredential())
                            .ConfigureRefresh(refresh =>
                            {
                                // All configuration values will be refreshed if the sentinel key changes.
                                refresh.Register("TestApp:Settings:Sentinel", refreshAll: true);
                            }).UseFeatureFlags((options) =>
                            {
                                options.Label = context.HostingEnvironment.ToString();
                            });
                        });
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
