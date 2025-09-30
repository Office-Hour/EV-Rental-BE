using Domain.Entities.Implement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.EntityConfigurations
{
    public class DomainBlobEntityConfiguration : IEntityTypeConfiguration<DomainBlob>
    {
        public void Configure(EntityTypeBuilder<DomainBlob> builder)
        {
            builder.ToTable("DomainBlob");
            builder.HasKey(e => e.Id);
            builder.Property(x => x.ContainerName)
                .IsRequired();
            builder.Property(x => x.BlobName)
                .HasMaxLength(200);
        }
    }
}
