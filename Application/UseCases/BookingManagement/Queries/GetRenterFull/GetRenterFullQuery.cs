using Application.DTOs.Profile;
using MediatR;

namespace Application.UseCases.BookingManagement.Queries.GetRenterFull;

public class GetRenterFullQuery : IRequest<List<RenterProfileDto>>
{
}
