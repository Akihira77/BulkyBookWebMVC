using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using BulkyBook.Utility;
using Stripe;
using BulkyBook.Models.ViewModels;
using BulkyBookWeb.Areas.Customer.Controllers;
using BulkyBook.DataAccess.DbInitializer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")
            ));
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// kalau mau buat custom identity make .AddIdentity
// kalau ngga biar saja make AddDefaultIdentity
//builder.Services.AddDefaultIdentity<IdentityUser>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddDefaultTokenProviders()
    .AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddRazorPages();
builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = "984578472506101";
    options.AppSecret = "2ab68a84017ee444d8600ff48202432d";
});
// jika ingin menambahkan custom url
// jika tidak menggunakan MapRazorPages

//builder.Services.ConfigureApplicationCookie(
//    options =>
//    {
//        options.LoginPath = $"/Identity/Account/Login";
//        options.LogoutPath = $"/Identity/Account/Logout";
//        options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
//    });

// opsi dalam menggunakan session pada browser
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

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
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
await SeedDatabase();
app.UseAuthentication();

app.UseAuthorization();
// untuk menggunakan session pada browser
app.UseSession();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
app.Run();


async Task SeedDatabase()
{
    using var scope = app.Services.CreateScope();
    var dbInitializer = scope.ServiceProvider
                        .GetRequiredService<IDbInitializer>();   
    await dbInitializer.Initialize();
}