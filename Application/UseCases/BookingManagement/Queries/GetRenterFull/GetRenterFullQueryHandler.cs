using Application.DTOs.Profile;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.BookingManagement.Queries.GetRenterFull;

public class GetRenterFullQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<GetRenterFullQuery, List<RenterProfileDto>>
{
    public async Task<List<RenterProfileDto>> Handle(GetRenterFullQuery request, CancellationToken cancellationToken)
    {
        var renters = await uow.Repository<Domain.Entities.BookingManagement.Renter>()
            .AsQueryable()
            .ToListAsync(cancellationToken);
        var renterDtos = mapper.Map<List<RenterProfileDto>>(renters);

        // for each renter, load user to get username
        foreach (var renter in renters)
        {
            var user = await uow.Repository<IdentityUser>().AsQueryable()
                .FirstOrDefaultAsync(u => u.Id == renter.UserId.ToString(), cancellationToken);
            var renterDto = renterDtos.FirstOrDefault(r => r.RenterId == renter.RenterId);
            if (renterDto != null && user != null)
            {
                renterDto.UserName = user.UserName ?? user.Email;
            }
        }

        return renterDtos;
    }
}
