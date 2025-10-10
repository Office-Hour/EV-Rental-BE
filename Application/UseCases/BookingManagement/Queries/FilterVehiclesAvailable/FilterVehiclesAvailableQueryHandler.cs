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
        if(request.StartTime != null && request.EndTime != null && request.StartTime >= request.EndTime)
        {
            throw new ValidationException("StartTime must be earlier than EndTime.");
        }
        var vehicleRepo = uow.Repository<VehicleAtStation>();
        var query = vehicleRepo.AsQueryable().Where(v => v.Status == VehicleAtStationStatus.Available && v.StationId == request.StationId);

        var repo = uow.Repository<Rental>();

        var rentalsNotAvailable = await repo.AsQueryable().Where(r => r.Status != RentalStatus.Cancelled && r.Status != RentalStatus.Completed).ToListAsync(cancellationToken: cancellationToken);
        var bookingsNotAvailable = await uow.Repository<Booking>().AsQueryable().Where(b => rentalsNotAvailable.Any(r => r.BookingId == b.BookingId)).ToListAsync(cancellationToken: cancellationToken);

        if (request.VehicleId != null)
        {
            // Filter by specific vehicle
            var vehicleAtStations = await vehicleRepo.AsQueryable().Where(v => v.VehicleId == request.VehicleId).ToListAsync(cancellationToken: cancellationToken);

            // Get only vehicles that are not in bookingsNotAvailable
            query = query.Where(v => vehicleAtStations.Any(vas => !bookingsNotAvailable.Any(b => b.VehicleAtStationId == vas.VehicleAtStationId)));
        }
        if (request.StartTime != null)
        {
            // Filter by start time
            query = query.Where(v => !bookingsNotAvailable.Any(b =>
                b.VehicleAtStationId == v.VehicleAtStationId &&
                ((request.StartTime < b.EndTime && request.StartTime >= b.StartTime) ||
                 (request.EndTime > b.StartTime && request.EndTime <= b.EndTime) ||
                 (request.StartTime <= b.StartTime && request.EndTime >= b.EndTime))));
        }
        if (request.EndTime != null)
        {
            // Filter by end time
            query = query.Where(v => !bookingsNotAvailable.Any(b =>
                b.VehicleAtStationId == v.VehicleAtStationId &&
                ((request.StartTime < b.EndTime && request.StartTime >= b.StartTime) ||
                 (request.EndTime > b.StartTime && request.EndTime <= b.EndTime) ||
                 (request.StartTime <= b.StartTime && request.EndTime >= b.EndTime))));
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
