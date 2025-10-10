﻿using Application.CustomExceptions;
using Application.DTOs.BookingManagement;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.BookingManagement;
using Domain.Entities.StationManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.BookingManagement.Queries.ViewVehicleDetails;

public class ViewVehicleDetailsQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<ViewVehicleDetailsQuery, VehicleDetailsDto>
{
    public async Task<VehicleDetailsDto> Handle(ViewVehicleDetailsQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await uow.Repository<Vehicle>().GetByIdAsync(request.VehicleId, cancellationToken) ?? throw new NotFoundException(errorMessage: $"Vehicle with ID {request.VehicleId} not found.");

        var vehicleAtStation = await uow.Repository<VehicleAtStation>().AsQueryable().Where(v => v.EndTime == null).FirstOrDefaultAsync(cancellationToken: cancellationToken) ?? throw new NotFoundException(errorMessage: "Vehicle is not found at any station");
        
        var bookingRepo = uow.Repository<Booking>();

        var vehicleBookings = await bookingRepo.AsQueryable()
            .Where(b => b.VehicleAtStationId == vehicleAtStation.VehicleAtStationId)
            .ToListAsync(cancellationToken);

        // Get pricing of today
        var pricing = await uow.Repository<Pricing>().AsQueryable()
            .Where(p => p.EffectiveFrom <= DateTime.UtcNow && (p.EffectiveTo == null || p.EffectiveTo > DateTime.UtcNow))
            .OrderByDescending(p => p.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException(errorMessage: "No active pricing found.");

        var bookingBriefDtos = mapper.Map<List<BookingBriefDto>>(vehicleBookings);

        var vehicleDetailsDto = mapper.Map<Vehicle, VehicleDetailsDto>(vehicle);

        vehicleDetailsDto.CurrentBatteryCapacityKwh = vehicleAtStation.CurrentBatteryCapacityKwh;
        vehicleDetailsDto.VehicleAtStationId = vehicleAtStation.VehicleAtStationId;
        vehicleDetailsDto.RentalPricePerHour = pricing.PricePerHour;
        vehicleDetailsDto.RentalPricePerDay = pricing.PricePerDay;
        vehicleDetailsDto.UpcomingBookings = bookingBriefDtos;

        return vehicleDetailsDto;
    }
}
