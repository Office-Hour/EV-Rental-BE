using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Identity;
using Domain.Entities.Implement.Aggregates.Identity_KyC;

namespace Persistence.EntityConfigurations;

public class DomainUserEntityConfiguration : IEntityTypeConfiguration<DomainUser>
{
    public void Configure(EntityTypeBuilder<DomainUser> b)
    {
        b.ToTable("DomainUser");
        b.HasKey(x => x.Id);

        // Config Foreign Key
        b.HasOne<IdentityUser>()
            .WithOne()
            .HasForeignKey<DomainUser>(x => x.IdentityUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
