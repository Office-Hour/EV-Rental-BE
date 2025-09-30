using Domain.Entities.Implement;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.BlobManagement.Queries.GetFile
{
    public class GetFileQuery : IRequest<DomainBlob>
    {
        public Guid BlobId { get; set; }
    }
}
