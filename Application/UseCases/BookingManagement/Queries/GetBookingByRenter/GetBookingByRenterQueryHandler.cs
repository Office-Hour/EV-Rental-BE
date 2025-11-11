using Application.CustomExceptions;
using Application.DTOs;
using Application.DTOs.BookingManagement;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.BookingManagement;
using Domain.Entities.StationManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.BookingManagement.Queries.GetBookingByRenter;

public class GetBookingByRenterQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<GetBookingByRenterQuery, PagedResult<BookingDetailsDto>>
{
    public async Task<PagedResult<BookingDetailsDto>> Handle(GetBookingByRenterQuery request, CancellationToken cancellationToken)
    {
        var renter = await uow.Repository<Renter>().GetByIdAsync(request.RenterId, cancellationToken) ?? throw new NotFoundException(errorMessage: "Renter not found");

        var query = uow.Repository<Booking>().AsQueryable().Where(b => b.RenterId == renter.RenterId);

        var totalItems = await query.CountAsync(cancellationToken: cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        var bookingDtos = mapper.Map<List<BookingDetailsDto>>(items);

        // calculate and set TotalPrice for each booking
        foreach (var bookingDto in bookingDtos)
        {
            // Get days and hours between StartTime and EndTime
            var booking = items.First(b => b.BookingId == bookingDto.BookingId);
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

        return new PagedResult<BookingDetailsDto>(
            Items: bookingDtos,
            Total: totalItems,
            Page: request.PageNumber,
            PageSize: request.PageSize
        );
    }
}
