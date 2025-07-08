using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using LoanManagementSystem.Data;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 🔌 Add services
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36))
    )
);

var app = builder.Build();

// 🌐 Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// 🚀 Add this Chrome auto-launch code (macOS specific)
if (app.Environment.IsDevelopment())
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Wait for server to be ready
                Task.Delay(2000).Wait();
                
                // Try multiple ways to open Chrome
                var chromePaths = new[]
                {
                    "/Applications/Google Chrome.app",
                    "/Applications/Google Chrome Canary.app",
                    "/Applications/Chromium.app"
                };

                foreach (var path in chromePaths)
                {
                    try
                    {
                        Process.Start("open", $"-a \"{path}\" \"{app.Urls.FirstOrDefault() ?? "http://localhost:5127"}\"");
                        break;
                    }
                    catch { /* Ignore errors */ }
                }
            }
        }
        catch { /* Silent fail - don't break app if browser doesn't open */ }
    });
}

app.Run();