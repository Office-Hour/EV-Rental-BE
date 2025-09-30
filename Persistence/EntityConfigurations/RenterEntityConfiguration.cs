using Domain.Entities.Implement.Aggregates.Identity_KyC;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations
{
    public class RenterEntityConfiguration : IEntityTypeConfiguration<Renter>
    {
        public void Configure(EntityTypeBuilder<Renter> builder)
        {
            builder.ToTable("Renter");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.LastName)
                .HasMaxLength(100);

            builder.HasOne(x => x.User)
                .WithOne(u => u.Renter)
                .HasForeignKey<Renter>(x => x.DomainUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
