using Data.Domain;
using Data;
using Data.Managers;
using Data.Repository.Services;
using Microsoft.AspNetCore.Identity;
using static Data.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
            options.EnableSensitiveDataLogging().UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDbContext"),
           sqlServerOptionsAction: sqlOptions =>
           {
               sqlOptions.EnableRetryOnFailure();
           }));



builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    options.User.RequireUniqueEmail = true;
})
  .AddEntityFrameworkStores<ApplicationDbContext>()
  .AddDefaultTokenProviders();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<IAccountManager, AccountManager>();
builder.Services.AddScoped<IDashboardManager, DashboardManager>();
builder.Services.AddScoped<IIdentityTokenService, IdentityTokenService>();
builder.Services.AddScoped<ILogExceptionService, LogExceptionService>();
builder.Services.AddScoped<ILogActivityService, LogActivityService>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddAutoMapper(typeof(Program));

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Index}/{id?}");

app.Run();
