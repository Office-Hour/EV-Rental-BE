using Domain.Entities.Implement;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.BlobManagement.Queries.GetFile
{
    public class GetFileQueryHandler : IRequestHandler<GetFileQuery, DomainBlob>
    {
        public async Task<DomainBlob> Handle(GetFileQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
