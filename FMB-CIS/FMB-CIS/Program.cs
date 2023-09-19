
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FMB_CIS.Data;
using FMB_CIS.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<LocalContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocalContext")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
        options.AccessDeniedPath = "/Forbidden/";
    }); //Added 2023-Sept-13

builder.Services.AddDistributedMemoryCache();

//builder.Services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<AppIdentityDbContext>().AddDefaultTokenProviders(); //Added 2023-09-19
//Token Validity
builder.Services.Configure<DataProtectionTokenProviderOptions>(opts => opts.TokenLifespan = TimeSpan.FromHours(10)); //Added 2023-09-19


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
}); //Added 2023-Sept-13 (Session)


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

app.UseAuthentication();//Added 2023-Sept-13
app.UseAuthorization();//Added 2023-Sept-13

app.UseSession();//Added 2023-Sept-13 (Session)

//app.MapRazorPages();//Added 2023-Sept-13
app.MapDefaultControllerRoute();//Added 2023-Sept-13

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
