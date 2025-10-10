using Application.DTOs;
using Application.DTOs.BookingManagement;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.StationManagement;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.BookingManagement.Queries.ViewVehiclesByStation;

public class ViewVehiclesByStationQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<ViewVehiclesByStationQuery, PagedResult<VehicleDto>>
{
    public async Task<PagedResult<VehicleDto>> Handle(ViewVehiclesByStationQuery request, CancellationToken cancellationToken)
    {
        var vehicleRepo = uow.Repository<VehicleAtStation>();
        var query = vehicleRepo.AsQueryable().Where(v => v.StationId == request.StationId && v.Status == VehicleAtStationStatus.Available);
        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        var dtos = mapper.Map<List<VehicleDto>>(items);
        return new PagedResult<VehicleDto>(dtos, totalItems, request.PageNumber, request.PageSize);
    }
}
