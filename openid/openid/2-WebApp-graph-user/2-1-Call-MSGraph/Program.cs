using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet_graph
{
    public static class Extension {
        public static void AddAmazonSecretsManager(this IConfigurationBuilder configurationBuilder, 
    					string region,
    					string secretName)
        {
            var configurationSource = 
                    new AmazonSecretsManagerConfigurationSource(region, secretName);

            configurationBuilder.Add(configurationSource);
        }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureAppConfiguration(((_, configurationBuilder) =>
                    {
                        configurationBuilder.AddAmazonSecretsManager("<your region>", "<secret name>");
                    }));
                    webBuilder.UseStartup<Startup>();
                });
    }
}
