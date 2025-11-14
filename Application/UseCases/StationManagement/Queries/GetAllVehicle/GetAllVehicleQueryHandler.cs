using Application.DTOs.BookingManagement;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.StationManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.StationManagement.Queries.GetAllVehicle;

public class GetAllVehicleQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<GetAllVehicleQuery, IEnumerable<VehicleDetailsDto>>
{
    public async Task<IEnumerable<VehicleDetailsDto>> Handle(GetAllVehicleQuery request, CancellationToken cancellationToken)
    {
        var vehicleRepo = uow.Repository<Vehicle>();
        var vehicles = await vehicleRepo.AsQueryable().ToListAsync(cancellationToken);
        if (vehicles.Count == 0)
        {
            return Enumerable.Empty<VehicleDetailsDto>();
        }

        var vehicleIds = vehicles.Select(v => v.VehicleId).ToList();
        var now = DateTime.UtcNow;

        var vehicleAtStations = await uow.Repository<VehicleAtStation>().AsQueryable()
            .Where(vs => vehicleIds.Contains(vs.VehicleId) && vs.EndTime == null)
            .ToListAsync(cancellationToken);

        var pricingRepo = uow.Repository<Pricing>();
        var activePricings = await pricingRepo.AsQueryable()
            .Where(p => vehicleIds.Contains(p.VehicleId)
                        && p.EffectiveFrom <= now
                        && (p.EffectiveTo == null || p.EffectiveTo > now))
            .ToListAsync(cancellationToken);

        var stationLookup = vehicleAtStations
            .GroupBy(vs => vs.VehicleId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(vs => vs.StartTime).First());

        var pricingLookup = activePricings
            .GroupBy(p => p.VehicleId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.EffectiveFrom).First());

        var dtos = mapper.Map<List<VehicleDetailsDto>>(vehicles);
        foreach (var dto in dtos)
        {
            if (stationLookup.TryGetValue(dto.VehicleId, out var station))
            {
                dto.VehicleAtStationId = station.VehicleAtStationId;
                dto.CurrentBatteryCapacityKwh = station.CurrentBatteryCapacityKwh;
                dto.Status = station.Status;
            }

            if (pricingLookup.TryGetValue(dto.VehicleId, out var pricing))
            {
                dto.RentalPricePerHour = pricing.PricePerHour;
                dto.RentalPricePerDay = pricing.PricePerDay;
                dto.DepositPrice = pricing.PricePerDay.HasValue
                    ? pricing.PricePerDay.Value * 0.2m
                    : pricing.PricePerHour * 24m * 0.2m;
            }
        }

        return dtos;
    }
}
