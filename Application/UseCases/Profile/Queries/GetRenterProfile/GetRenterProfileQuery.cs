using Application.DTOs.Profile;
using MediatR;

namespace Application.UseCases.Profile.Queries.GetRenterProfile;

public class GetRenterProfileQuery : IRequest<RenterProfileDto>
{
    public Guid UserId { get; set; }
}
