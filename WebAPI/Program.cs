using Application;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Scalar.AspNetCore;
using WebAPI;
using WebAPI.Behaviors;
using WebAPI.FilterException;
using WebAPI.Middlewares;
using Microsoft.OpenApi.Models; // added for Swagger/OpenAPI

var builder = WebApplication.CreateBuilder(args);

// Add FastEndpoints (if still using some endpoints)
//builder.Services.AddFastEndpoints();

// Add Controllers support
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
// Add Exception Handler.
builder.Services.AddExceptionHandler<ErrorExceptionHandler>();
builder.Services.AddProblemDetails();
// Add Pipeline Behavior for validation flow.
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// CORS: add a default policy (adjust AllowedOrigins as needed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Swagger / OpenAPI (Swashbuckle)
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EV Rental API",
        Version = "v1"
    });

    // JWT Bearer token support in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
    options.EnableAnnotations();
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCustomIdentity();
builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddCustomAuthorization();
builder.Services.AddOpenApiWithJwtBearer();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "EV Rental API";
        document.Info.Version = "v1";
        return Task.CompletedTask;
    });
});

var app = builder.Build();
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    // Enable Swashbuckle middleware (swagger.json + swagger UI)
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EV Rental API v1");
        c.RoutePrefix = "swagger"; // serves UI at /swagger; set to string.Empty to serve at root
    });

    app.MapOpenApi();
    app.MapScalarApiReference((options, httpContext) =>
    {
        var host = httpContext.Request.Host.HasValue
            ? httpContext.Request.Host.Value
            : "localhost";

        var pathBase = httpContext.Request.PathBase.HasValue
            ? httpContext.Request.PathBase.Value.TrimEnd('/')
            : string.Empty;

        var baseUrl = $"https://{host}{pathBase}".TrimEnd('/');

        options.Servers = new[]
        {
            new ScalarServer(baseUrl)
        };
    });
}

app.UseMiddleware<CustomMiddleware>();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseHsts();// Enables HTTP Strict Transport Security (HSTS) for the application.

// Enable CORS - must be before Authentication/Authorization and before endpoints
app.UseCors("DefaultCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Map Controllers (add this line)
app.MapControllers();

// Keep FastEndpoints if you're still using other endpoints
//app.UseFastEndpoints();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await services.Database.MigrateAsync();
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
}

// Chạy 1 lần thôi, lần sau comment lại 
await app.CreateStaffAccounts();

app.Run();
