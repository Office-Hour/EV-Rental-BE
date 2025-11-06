using Application.DTOs.BookingManagement;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.BookingManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.BookingManagement.Queries.GetBookingFull;

public class GetBookingFullQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<GetBookingFullQuery, List<BookingDetailsDto>>
{
    public async Task<List<BookingDetailsDto>> Handle(GetBookingFullQuery request, CancellationToken cancellationToken)
    {
        var bookings = await uow.Repository<Booking>()
            .AsQueryable()
            .ToListAsync(cancellationToken);
        return mapper.Map<List<BookingDetailsDto>>(bookings);
    }
}
