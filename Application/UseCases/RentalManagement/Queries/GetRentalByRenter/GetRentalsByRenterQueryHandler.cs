using Application.DTOs;
using Application.DTOs.RentalManagement;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities.RentalManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.RentalManagement.Queries.GetRentalByRenter;

public class GetRentalsByRenterQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<GetRentalsByRenterQuery, PagedResult<RentalDto>>
{
    public async Task<PagedResult<RentalDto>> Handle(GetRentalsByRenterQuery request, CancellationToken cancellationToken)
    {
        // Base query on Rentals; no Include is necessary if we ProjectTo DTOs
        var rentalsQuery = uow.Repository<Rental>()
            .AsQueryable()
            .Where(r => r.Booking != null && r.Booking.RenterId == request.RenterId)
            .AsNoTracking();

        // Count after filtering
        var totalCount = await rentalsQuery.CountAsync(cancellationToken);

        // Stable sorting for pagination (adjust field to your domain: StartTime/CreatedAt/Id)
        rentalsQuery = rentalsQuery.OrderByDescending(r => r.StartTime);

        // Project directly to DTO to avoid materializing full entities
        var rentalDtos = await rentalsQuery
            .ProjectTo<RentalDto>(mapper.ConfigurationProvider)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<RentalDto>(rentalDtos, totalCount, request.PageNumber, request.PageSize);
    }
}
