using Blog.Domain.Enums;
using Blog.Infrastructure.Extensions;
using Blog.MVC.Auth;
using Blog.MVC.Validation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/account/signin";
        options.AccessDeniedPath = "/";
        options.Cookie.Name = "auth";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    }
);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireConfirmedEmail", policy =>
        policy.AddRequirements(new ConfirmedEmailAddressRequirement())
    );
    options.AddPolicy("RequireCritic", policy =>
        policy.RequireRole(UserRoleEnum.Critic.ToString())
    );
    options.AddPolicy("RequireEditor", policy =>
        policy.RequireRole(UserRoleEnum.Editor.ToString())
    );
    options.AddPolicy("RequireCommentator", policy =>
        policy.RequireRole(UserRoleEnum.Commentator.ToString())
    );
    options.AddPolicy("RequireAdmin", policy => 
        policy.RequireRole(UserRoleEnum.Admin.ToString())
    );
});

builder.Services.AddInfrastructureServices(builder.Configuration);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IValidationAttributeAdapterProvider, CustomValidationAttributeAdapterProvider>();

builder.Services.AddSingleton<IAuthorizationHandler, RequireConfirmedEmailAddressHandler>();

builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationResultHandler>();

var app = builder.Build();

await app.InitializeInfrastructureServicesAsync(app.Configuration);

// Configure the HTTP request pipeline.
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
