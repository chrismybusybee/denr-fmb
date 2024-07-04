
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FMB_CIS.Data;
using FMB_CIS.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using FMB_CIS;
using reCAPTCHA.AspNetCore;
using FluentValidation.AspNetCore;
using System.Reflection;
using FMB_CIS.Interface;
using FMB_CIS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation().AddFluentValidation(options =>
{
    // Validate child properties and root collection elements
    options.ImplicitlyValidateChildProperties = true;
    options.ImplicitlyValidateRootCollectionElements = true;

    // Automatic registration of validators in assembly
    options.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
});


builder.Services.AddDbContext<LocalContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CISDB")));

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
// Configure reCAPTCHA settings from appsettings.json
builder.Services.Configure<RecaptchaSettings>(builder.Configuration.GetSection("RecaptchaSettings")); //Added 2023-Oct-5

// Add reCAPTCHA services
builder.Services.AddRecaptcha(builder.Configuration.GetSection("RecaptchaSettings")); //Added 2023-Oct-5

builder.Services.Configure<GoogleCaptchaConfig>(builder.Configuration.GetSection("GoogleReCaptcha")); //Added 2023-Oct-6
builder.Services.Configure<SMTPCredentials>(builder.Configuration.GetSection("SMTPCredentials")); //Added 2024-Feb-28
builder.Services.Configure<SMTPCredentialsSecondary>(builder.Configuration.GetSection("SMTPCredentialsSecondary")); //Added 2024-Feb-28
builder.Services.AddTransient(typeof(GoogleCaptchaService));

// Services
builder.Services.AddScoped<INotificationAbstract, NotificationService>();
builder.Services.AddScoped<IWorkflowAbstract, WorkflowService>();
builder.Services.AddScoped<IApplicationAbstract, ApplicationService>();
builder.Services.AddScoped<IWorkflowStepAbstract, WorkflowStepService>();
builder.Services.AddScoped<IWorkflowNextStepAbstract, WorkflowNextStepService>();
builder.Services.AddScoped<IPermitStepCount, PermitStepCountService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
}); //Added 2023-Sept-13 (Session)

//Added 2023-Sept-26 (Email Sender)
builder.Services.AddTransient<IEmailSender, MailKitEmailSender>();
builder.Services.Configure<MailKitEmailSenderOptions>(options =>
{
    //options.Host_Address = "smtp-relay.sendinblue.com";
    //options.Host_Port = 587;
    //options.Host_Username = "support@mybusybee.net";
    //options.Host_Password = "A0M26PbZUN5TYF3s";
    //options.Sender_EMail = "support@mybusybee.net";
    //options.Sender_Name = "FMB-CIS Bot";

    //try
    //{
    //    options.Host_Address = "smtp-relay.brevo.com";
    //    options.Host_Port = 587;
    //    options.Host_Username = "support@mybusybee.net";
    //    options.Host_Password = "OBAq9p8GtLM5g2KS";
    //    options.Sender_EMail = "support@mybusybee.net";
    //    options.Sender_Name = "FMB-CIS Bot";

    //}

    //catch
    //{
    //    options.Host_Address = "smtp-relay.brevo.com";
    //    options.Host_Port = 587;
    //    options.Host_Username = "franz@mybusybee.net";
    //    options.Host_Password = "IJg2qdh8y9WSCcUZ";
    //    options.Sender_EMail = "franz@mybusybee.net";
    //    options.Sender_Name = "FMB-CIS Bot";
    //}

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

app.UseAuthentication();//Added 2023-Sept-13
app.UseAuthorization();//Added 2023-Sept-13

app.UseSession();//Added 2023-Sept-13 (Session)

//app.MapRazorPages();//Added 2023-Sept-13
app.MapDefaultControllerRoute();//Added 2023-Sept-13

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
