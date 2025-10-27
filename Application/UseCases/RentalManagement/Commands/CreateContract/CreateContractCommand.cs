using Domain.Enums;
using MediatR;

namespace Application.UseCases.RentalManagement.Commands.CreateContract;

public class CreateContractCommand : IRequest<Guid>
{
    public Guid RentalId { get; set; }
    public EsignProvider Provider { get; set; }
}
