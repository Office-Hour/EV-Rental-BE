using Application.CustomExceptions;
using Application.DTOs.BookingManagement;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.BookingManagement;
using Domain.Entities.StationManagement;
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
        var bookingDtos = mapper.Map<List<BookingDetailsDto>>(bookings);

        foreach (var bookingDto in bookingDtos)
        {
            // Get days and hours between StartTime and EndTime
            var booking = bookings.First(b => b.BookingId == bookingDto.BookingId);
            var duration = (booking.EndTime - booking.StartTime).TotalHours;
            var days = Math.Floor(duration / 24);
            var extraHours = duration % 24;

            var vehicle = await uow.Repository<VehicleAtStation>()
                .AsQueryable()
                .Where(v => v.VehicleAtStationId == booking.VehicleAtStationId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Vehicle not found");

            var price = await uow.Repository<Pricing>()
                .AsQueryable()
                .Where(p => p.VehicleId == vehicle.VehicleId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Price not found");

            var totalAmount = (decimal)days * price.PricePerDay + (decimal)extraHours * price.PricePerHour;

            bookingDto.TotalAmount = (decimal)totalAmount!;

            var depositFee = await uow.Repository<Fee>()
                .AsQueryable()
                .Where(d => d.BookingId == bookingDto.BookingId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Deposit fee not found");

            bookingDto.DepositAmount = depositFee.Amount;
        }

        return bookingDtos;
    }
}
