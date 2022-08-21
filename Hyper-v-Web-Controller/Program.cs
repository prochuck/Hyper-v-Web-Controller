using Hyper_v_Web_Controller;
using Hyper_v_Web_Controller.Interfaces;
using Hyper_v_Web_Controller.Models;
using Hyper_v_Web_Controller.Services;
using Microsoft.EntityFrameworkCore;
using Hyper_v_Web_Controller.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IHyperVThing, HyperVThing>();
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDBContext>(e => e.UseSqlServer(@"Data Source=(local);Database=HyperVWebController;Trusted_Connection=True;"));
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IVMImageRepository, VMImageRepository>();
builder.Services.AddTransient<IVMRepository, VMRepository>();
builder.Services.AddTransient<IHashService, HashService>();
builder.Services.AddHttpsRedirection(e =>
{
    e.HttpsPort = 55556;
});

builder.Services.AddAuthentication("Cookies").AddCookie(o =>
{
    o.ExpireTimeSpan = TimeSpan.FromHours(1);
    o.LoginPath = "/Authentication/Login";
    o.LogoutPath = "/Authentication/Logout";
});

builder.WebHost.UseUrls("http://*:55555", "https://*:55556");

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

app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();


app.UseEndpoints(endpoints => endpoints.MapControllers());


app.Run();

