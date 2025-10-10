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

        // Filter out vehicles that have active bookings (not cancelled/completed) overlapping with the requested time window
        var bookingRepo = uow.Repository<Booking>();

        if (request.VehicleId != null)
        {
            query = query.Where(v => v.VehicleId == request.VehicleId);
        }

        if (request.StartTime != null && request.EndTime != null)
        {
            // Exclude vehicles that have bookings overlapping with the requested time window and are not cancelled/completed
            query = query.Where(v =>
                !bookingRepo.AsQueryable().Any(b =>
                    b.VehicleAtStationId == v.VehicleAtStationId &&
                    b.Rental.Status != RentalStatus.Cancelled &&
                    b.Rental.Status != RentalStatus.Completed &&
                    (
                        (request.StartTime < b.EndTime && request.StartTime >= b.StartTime) ||
                        (request.EndTime > b.StartTime && request.EndTime <= b.EndTime) ||
                        (request.StartTime <= b.StartTime && request.EndTime >= b.EndTime)
                    )
                )
            );
        }
        // If only StartTime or EndTime is provided, apply partial overlap logic
        else if (request.StartTime != null)
        {
            query = query.Where(v =>
                !bookingRepo.AsQueryable().Any(b =>
                    b.VehicleAtStationId == v.VehicleAtStationId &&
                    b.Rental.Status != RentalStatus.Cancelled &&
                    b.Rental.Status != RentalStatus.Completed &&
                    (request.StartTime < b.EndTime && request.StartTime >= b.StartTime)
                )
            );
        }
        else if (request.EndTime != null)
        {
            query = query.Where(v =>
                !bookingRepo.AsQueryable().Any(b =>
                    b.VehicleAtStationId == v.VehicleAtStationId &&
                    b.Rental.Status != RentalStatus.Cancelled &&
                    b.Rental.Status != RentalStatus.Completed &&
                    (request.EndTime > b.StartTime && request.EndTime <= b.EndTime)
                )
            );
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
