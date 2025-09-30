using Domain.Entities.Abstract;
using Domain.Entities.Implement.Aggregates.Identity_KyC;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Persistence;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext()
    {

    }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }
    public DbSet<DomainUser> DomainUsers { get; set; }
    public DbSet<Renter> Renters { get; set; }
    public DbSet<VerificationAudit> KycReviews { get; set; }
    public DbSet<Document> KycDocuments { get; set; }

    private static string ConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.db.json", optional: false, reloadOnChange: true)
            .Build();
        return configuration.GetConnectionString("DefaultConnection") ?? throw new NullReferenceException("ConnectionString not found");
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(ConnectionString(), options =>
            {
                options.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        }
        optionsBuilder.UseLazyLoadingProxies();
        optionsBuilder
            .UseAsyncSeeding(async (ctx, _, cancellationToken) =>
            {
                var roles = new[] { "Admin", "Staff", "User" };
                foreach (var role in roles)
                {
                    if (!await ctx.Set<IdentityRole>().AnyAsync(r => r.Name == role, cancellationToken))
                    {
                        await ctx.Set<IdentityRole>().AddAsync(new IdentityRole(role), cancellationToken);
                    }
                }

                await ctx.SaveChangesAsync(cancellationToken);
            })
            .UseSeeding((ctx, _) =>
            {
                var roles = new[] { "Admin", "Staff", "User" };
                foreach (var role in roles)
                {
                    if (!ctx.Set<IdentityRole>().Any(r => r.Name == role))
                    {
                        ctx.Set<IdentityRole>().Add(new IdentityRole(role));
                    }
                }
                ctx.SaveChanges();
            });
        base.OnConfiguring(optionsBuilder);
    }

    public override int SaveChanges()
    {
        var entries = ChangeTracker.Entries();
        foreach (var entry in entries)
        {
            if (entry.Entity is BaseEntity && (entry.State == EntityState.Added || entry.State == EntityState.Modified))
            {
                var entity = entry.Entity as BaseEntity;
                if (entry.State == EntityState.Added)
                {
                    entity!.CreatedAt = DateTime.UtcNow;
                    entity.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity!.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries();
        foreach (var entry in entries)
        {
            if (entry.Entity is BaseEntity && (entry.State == EntityState.Added || entry.State == EntityState.Modified))
            {
                var entity = entry.Entity as BaseEntity;
                if (entry.State == EntityState.Added)
                {
                    entity!.CreatedAt = DateTime.UtcNow;
                    entity.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity!.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
