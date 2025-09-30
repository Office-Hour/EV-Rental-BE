using Domain.Enums;
using MediatR;

namespace Application.UseCases.Kyc.Commands.UploadIdentityDocument
{
    public class UploadIdentityDocumentCommand : IRequest<Guid>
    {
        public Guid RenterId { get; set; }
        public Guid FrontImageId { get; set; }
        public Guid BackImageId { get; set; }
        public DocumentType DocumentType { get; set; }
    }
}
