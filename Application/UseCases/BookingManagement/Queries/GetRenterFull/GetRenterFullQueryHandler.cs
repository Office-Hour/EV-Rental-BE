using Application.DTOs.Profile;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.BookingManagement.Queries.GetRenterFull;

public class GetRenterFullQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<GetRenterFullQuery, List<RenterProfileDto>>
{
    public async Task<List<RenterProfileDto>> Handle(GetRenterFullQuery request, CancellationToken cancellationToken)
    {
        var renters = await uow.Repository<Domain.Entities.BookingManagement.Renter>()
            .AsQueryable()
            .ToListAsync(cancellationToken);
        return mapper.Map<List<RenterProfileDto>>(renters);
    }
}
