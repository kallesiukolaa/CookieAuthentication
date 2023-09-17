using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CookieAuthentication
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
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.ConfigureAppConfiguration(((_, configurationBuilder) =>
        {
            configurationBuilder.AddAmazonSecretsManager("<your region>", "<secret name>");
        }));

        var settings = new MyApiCredentials();
        var b = builder.Configuration.GetSection("AzureAd");

        //set CookieAuthenticationDefaults.AuthenticationScheme as the default authentication schemeö

        // Add microsoft sign in page
        builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();

        builder.Services.Configure<MyApiCredentials>(builder.Configuration);

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(x => x.LoginPath = "/account/login");

        builder.Services.AddAuthentication()
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("TEST"), OpenIdConnectDefaults.AuthenticationScheme, "ADCookies");

        var app = builder.Build();
        var a = builder.Configuration.GetSection("TEST");

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.Run();
    }
}
}
