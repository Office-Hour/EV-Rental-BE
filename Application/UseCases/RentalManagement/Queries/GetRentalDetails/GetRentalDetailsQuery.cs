using MediatR;

namespace Application.UseCases.RentalManagement.Queries.GetRentalDetails;

public class GetRentalDetailsQuery : IRequest<RentalDetailsDto>
{
    public Guid RentalId { get; set; }
}
