using Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using WebApp;
using WebApp.UIAuthService;
using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);

// ? Register PaymentSimulationService
builder.Services.AddScoped<PaymentSimulationService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddCustomIdentity()
                .AddCustomAuthentication(builder.Configuration)
                .AddCustomAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUiAuthService, UiAuthService>(); // <-- add this adapter

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapRazorPages()
   .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await services.Database.MigrateAsync();
}

app.Run();