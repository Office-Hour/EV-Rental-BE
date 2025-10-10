using Application.CustomExceptions;
using Application.DTOs.Profile;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.BookingManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.Profile.Queries.GetRenterProfile;

public class GetRenterProfileQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<GetRenterProfileQuery, RenterProfileDto>
{
    public async Task<RenterProfileDto> Handle(GetRenterProfileQuery request, CancellationToken cancellationToken)
    {
        var renterRepo = uow.Repository<Renter>();
        var renter = await renterRepo.AsQueryable()
            .FirstOrDefaultAsync(r => r.UserId == request.UserId, cancellationToken: cancellationToken)
        ?? throw new NotFoundException("Renter not found");
        return mapper.Map<RenterProfileDto>(renter);
    }
}
