using MediatR;

namespace Application.UseCases.BlobManagement.Commands.CreateBlob
{
    public class CreateBlobCommandHandler : IRequestHandler<CreateBlobCommand, Guid>
    {
        public async Task<Guid> Handle(CreateBlobCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
