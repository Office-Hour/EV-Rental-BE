using Application.DTOs.RentalManagement;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.RentalManagement.Queries.GetRentalByBookingId;

public class GetRentalByBookingIdQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<GetRentalByBookingIdQuery, RentalDto?>
{
    public async Task<RentalDto?> Handle(GetRentalByBookingIdQuery request, CancellationToken cancellationToken)
    {
        var rentalRepository = uow.Repository<Domain.Entities.RentalManagement.Rental>();
        var rental = await rentalRepository
            .AsQueryable()
            .FirstOrDefaultAsync(r => r.BookingId == request.BookingId, cancellationToken);
        if (rental == null)
        {
            return null;
        }
        var rentalDto = mapper.Map<RentalDto>(rental);
        return rentalDto;
    }
}
