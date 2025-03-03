using System.ComponentModel.Design;
using Microsoft.AspNetCore.Identity;
using MVC_LapinCouvert.Data;
using Microsoft.EntityFrameworkCore;
using Models.Interface;
using static Models.Interface.ServiceBaseEf;
using JuliePro.Utility;
using MVC_LapinCouvert.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseLazyLoadingProxies();
});

//var connectionstring = builder.Configuration.GetConnectionString("applicationdbcontext") ?? throw new InvalidOperationException("connection string 'applicationdbcontext' not found.");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//{
//    // change to sqlite
//    options.UseSqlite(connectionstring);
//    options.UseLazyLoadingProxies();
//});

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped(typeof(IServiceBaseAsync<>), typeof(ServiceBaseEF<>));
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ClientsService>();
builder.Services.AddSingleton<IImageFileManager, ImageFileManager>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
