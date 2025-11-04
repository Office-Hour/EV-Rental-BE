using Application.CustomExceptions;
using Application.DTOs.BookingManagement;
using Application.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.UseCases.BookingManagement.Queries.GetBookingDetails;

public class GetBookingDetailsQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<GetBookingDetailsQuery, BookingDetailsDto>
{
    public async Task<BookingDetailsDto> Handle(GetBookingDetailsQuery request, CancellationToken cancellationToken)
    {
        var booking = await uow.Repository<Domain.Entities.BookingManagement.Booking>()
            .GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new NotFoundException("Booking not found.");
        return mapper.Map<BookingDetailsDto>(booking);
    }
}
