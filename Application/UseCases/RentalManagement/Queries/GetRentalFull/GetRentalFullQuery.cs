using Application.DTOs.RentalManagement;
using MediatR;

namespace Application.UseCases.RentalManagement.Queries.GetRentalFull;

public class GetRentalFullQuery : IRequest<List<RentalDetailsDto>>
{
}
