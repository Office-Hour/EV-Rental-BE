using Application.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Data;
using Persistence.External;

namespace Persistence
{
    public static class PersistenceDependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Default Connection not found");
            // Add your persistence-related services here
            // For example, you might want to add DbContext, repositories, etc.
             services.AddDbContext<ApplicationDbContext>(options =>
                 options.UseSqlServer(connectionString));

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.Configure<BlobStorageOptions>(opt =>
            {
                var cfg = configuration.GetSection("AzureBlob");
                opt.DefaultAccess = Enum.TryParse(cfg["DefaultAccess"], out PublicAccessType access) ? access : PublicAccessType.None;
                opt.UseSasUrls = bool.TryParse(cfg["UseSasUrls"], out var useSas) ? useSas : true;
                if (int.TryParse(cfg["SasTtlMinutes"], out var mins))
                    opt.SasTtl = TimeSpan.FromMinutes(mins);
            });

            // Create BlobServiceClient from connection string
            services.AddSingleton(sp =>
            {
                var cs = configuration.GetSection("AzureBlob")["ConnectionString"];
                return new BlobServiceClient(cs);
            });

            // Register service
            services.AddScoped<IBlobStorageService, BlobStorageService>();


            return services;
        }
    }
}
