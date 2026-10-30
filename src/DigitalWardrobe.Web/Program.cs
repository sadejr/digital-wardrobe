using DigitalWardrobe.Core.Interfaces;
using DigitalWardrobe.Core.Models;
using DigitalWardrobe.Core.Services;
using DigitalWardrobe.Data;
using DigitalWardrobe.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using DigitalWardrobe.Data.Storage;
using DigitalWardrobe.Data.Images;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'Default' is not configured. Set it with user secrets or an environment variable.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// User accounts and login
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Application services
var limits = builder.Configuration.GetSection("Limits").Get<WardrobeLimits>() ?? new WardrobeLimits();
builder.Services.AddSingleton(limits);
builder.Services.AddScoped<IClothingItemRepository, ClothingItemRepository>();
builder.Services.AddScoped<ClothingItemService>();
var photosPath = Path.Combine(
    builder.Environment.ContentRootPath,
    builder.Configuration["Storage:PhotosPath"] ?? "photos");
builder.Services.AddSingleton<IFileStorage>(new LocalFileStorage(photosPath));
builder.Services.AddSingleton<IImageProcessor, SkiaImageProcessor>();

// Pages
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
}); 
builder.Services.AddRazorPages();

var app = builder.Build();

// Apply migrations on startup (enabled in Docker only)
if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Request pipeline: the order of these steps matters
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Block the registration page when registration is turned off
if (!app.Configuration.GetValue<bool>("Registration:Enabled"))
{
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/Identity/Account/Register"))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        await next();
    });
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();