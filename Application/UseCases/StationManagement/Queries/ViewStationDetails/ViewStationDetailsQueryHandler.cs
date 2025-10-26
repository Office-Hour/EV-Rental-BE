using Application.CustomExceptions;
using Application.DTOs.BookingManagement;
using Application.DTOs.StationManagement;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.StationManagement;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.StationManagement.Queries.ViewStationDetails;

public class ViewStationDetailsQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<ViewStationDetailsQuery, StationDetailsDto>
{
    public async Task<StationDetailsDto> Handle(ViewStationDetailsQuery request, CancellationToken cancellationToken)
    {
        var station = await uow.Repository<Station>()
            .GetByIdAsync(request.StationId, cancellationToken)
            ?? throw new NotFoundException("Station not found.");

        var vehicleRepo = uow.Repository<VehicleAtStation>();
        var query = vehicleRepo.AsQueryable().Where(v => v.StationId == request.StationId && v.Status == VehicleAtStationStatus.Available);
        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        var dtos = mapper.Map<List<VehicleDto>>(items);

        return new StationDetailsDto 
        {
            Id = station.StationId,
            Name = station.Name,
            Address = station.Address,
            Capacity = totalItems,
            Vehicles = dtos
        };
    }
}
