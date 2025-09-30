
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Application;
using MediatR;
using WebAPI.Behaviors;
using WebAPI.FilterException;
using WebAPI.Middlewares;
using Persistence;
using Scalar.AspNetCore;
using FastEndpoints;

namespace WebAPI;

public class Program
{
    public static void Main(string[] args)
    {
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

        app.Run();
    }
}
