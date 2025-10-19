using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Portfolio.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Portfolio;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddDbContext<PortfolioContext>(options => options.UseSqlServer(connectionString));

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // 2️⃣ Localization setup
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

        builder.Services.AddControllersWithViews();

        builder.Services.AddRazorPages();

        // 3️⃣ Authentication setup
        builder.Services.AddAuthentication("PortfolioAuth").AddCookie("PortfolioAuth", options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
            });

        var app = builder.Build();

        // 4️⃣ Supported cultures
        var supportedCultures = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("ar-JO")
        };

        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("en-US"),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures
        });

        // 5️⃣ Middleware pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        // ✅ Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // 6️⃣ Default route
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();

        app.Run();
    }
}
