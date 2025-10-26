using Application.DTOs;
using Application.DTOs.BookingManagement;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.BookingManagement;
using Domain.Entities.RentalManagement;
using Domain.Entities.StationManagement;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.BookingManagement.Queries.FilterVehiclesAvailable;

public class FilterVehiclesAvailableQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<FilterVehiclesAvailableQuery, PagedResult<VehicleDto>>
{
    public async Task<PagedResult<VehicleDto>> Handle(FilterVehiclesAvailableQuery request, CancellationToken cancellationToken)
    {
        if (request.StartTime != null && request.EndTime != null && request.StartTime >= request.EndTime)
            throw new ValidationException("StartTime must be earlier than EndTime.");

        var vehicleRepo = uow.Repository<VehicleAtStation>().AsQueryable();
        var rentalRepo = uow.Repository<Rental>().AsQueryable();
        var bookingRepo = uow.Repository<Booking>().AsQueryable();

        // Base: only available vehicles at the station
        IQueryable<VehicleAtStation> query = vehicleRepo
            .Where(v => v.Status == VehicleAtStationStatus.Available
                        && v.StationId == request.StationId);

        // Optional: filter by specific vehicle
        if (request.VehicleId != null)
        {
            query = query.Where(v => v.VehicleId == request.VehicleId);
        }

        // Subquery: rentals that are still "active" (i.e., not Cancelled/Completed)
        var activeRentalBookingIds = rentalRepo
            .Where(r => r.Status != RentalStatus.Cancelled && r.Status != RentalStatus.Completed)
            .Select(r => r.BookingId);

        // Subquery: bookings linked to active rentals
        var activeBookings = bookingRepo
            .Where(b => activeRentalBookingIds.Contains(b.BookingId));

        // Time overlap logic: overlap exists iff (reqStart < b.End) && (reqEnd > b.Start)
        // If only one bound is provided, degrade sensibly.
        if (request.StartTime != null && request.EndTime != null)
        {
            var s = request.StartTime.Value;
            var e = request.EndTime.Value;

            query = query.Where(v => !activeBookings.Any(b =>
                b.VehicleAtStationId == v.VehicleAtStationId
                && s < b.EndTime
                && e > b.StartTime));
        }
        else if (request.StartTime != null) // only start provided → exclude anything that ends after start
        {
            var s = request.StartTime.Value;
            query = query.Where(v => !activeBookings.Any(b =>
                b.VehicleAtStationId == v.VehicleAtStationId
                && s < b.EndTime));
        }
        else if (request.EndTime != null) // only end provided → exclude anything that starts before end
        {
            var e = request.EndTime.Value;
            query = query.Where(v => !activeBookings.Any(b =>
                b.VehicleAtStationId == v.VehicleAtStationId
                && e > b.StartTime));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = mapper.Map<List<VehicleDto>>(items);
        return new PagedResult<VehicleDto>(dtos, totalItems, request.PageNumber, request.PageSize);
    }
}
