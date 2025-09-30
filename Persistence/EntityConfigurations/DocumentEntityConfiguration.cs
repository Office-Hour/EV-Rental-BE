using Domain.Entities.Implement.Aggregates.Identity_KyC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class DocumentEntityConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Document");

        builder.HasKey(e => e.Id);

        builder.Property(x => x.Notes)
            .HasMaxLength(200);

        //
        builder.HasOne<Renter>()
            .WithMany(r => r.Documents)
            .HasForeignKey(r => r.RenterId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.FrontImage)
            .WithOne()
            .HasForeignKey<Document>(d => d.FrontImageId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(d => d.BackImage)
            .WithOne()
            .HasForeignKey<Document>(d => d.BackImageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
