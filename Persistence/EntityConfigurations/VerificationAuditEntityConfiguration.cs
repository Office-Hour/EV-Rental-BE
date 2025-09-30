using Domain.Entities.Implement.Aggregates.Identity_KyC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Persistence.EntityConfigurations
{
    public class VerificationAuditEntityConfiguration : IEntityTypeConfiguration<VerificationAudit>
    {
        public void Configure(EntityTypeBuilder<VerificationAudit> builder)
        {
            builder.ToTable("VerificationAudit");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Notes)
                .HasMaxLength(200);

            builder.HasOne(x => x.Reviewer)
                .WithMany(x => x.VerificationAudits)
                .HasForeignKey(x => x.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Renter)
                .WithMany(r => r.VerificationAudits)
                .HasForeignKey(x => x.RenterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
