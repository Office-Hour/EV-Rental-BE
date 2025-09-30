using Application.UseCases.BlobManagement.Queries.GetFile;
using MediatR;

namespace Application.UseCases.Kyc.Commands.UploadIdentityDocument;

public class UploadIdentityDocumentCommandHandler : IRequestHandler<UploadIdentityDocumentCommand, Guid>
{
    public async Task<Guid> Handle(UploadIdentityDocumentCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
