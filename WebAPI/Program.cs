using Application;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Scalar.AspNetCore;
using WebAPI;
using WebAPI.Behaviors;
using WebAPI.FilterException;
using WebAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddFastEndpoints();

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
// Add Exception Handler.
builder.Services.AddExceptionHandler<ErrorExceptionHandler>();
builder.Services.AddProblemDetails();
// Add Pipeline Behavior for validation flow.
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCustomIdentity();
builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddCustomAuthorization();
builder.Services.AddOpenApiWithJwtBearer();

var app = builder.Build();
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddleware<CustomMiddleware>();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseHsts();// Enables HTTP Strict Transport Security (HSTS) for the application.

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await services.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoles] WHERE [NormalizedName]='ADMIN')
INSERT [dbo].[AspNetRoles] ([Id],[Name],[NormalizedName],[ConcurrencyStamp])
VALUES ('90000000-0000-0000-0000-000000000001','Admin','ADMIN','c0c2f3f6-38c3-4a3f-8a8f-8f3a0a5b1111');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoles] WHERE [NormalizedName]='STAFF')
INSERT [dbo].[AspNetRoles] ([Id],[Name],[NormalizedName],[ConcurrencyStamp])
VALUES ('90000000-0000-0000-0000-000000000002','Staff','STAFF','d1d2e3e4-45f6-47a1-9c9a-2b2c3d4e2222');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoles] WHERE [NormalizedName]='RENTER')
INSERT [dbo].[AspNetRoles] ([Id],[Name],[NormalizedName],[ConcurrencyStamp])
VALUES ('90000000-0000-0000-0000-000000000003','Renter','RENTER','e2e3f4f5-56a7-48b2-8d8e-3c3d4e5f3333');
");
    await services.Database.MigrateAsync();
}

app.Run();
