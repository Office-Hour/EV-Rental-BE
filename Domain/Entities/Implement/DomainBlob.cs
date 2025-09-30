using Domain.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Implement
{
    public class DomainBlob : BaseEntity
    {
        public string ContainerName { get; set; } = null!;
        public string BlobName { get; set; } = null!;
    }
}
