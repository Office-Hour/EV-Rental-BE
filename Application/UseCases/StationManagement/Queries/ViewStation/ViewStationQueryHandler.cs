using Application.DTOs;
using Application.Interfaces;
using Domain.Entities.StationManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Application.UseCases.StationManagement.Queries.ViewStation;

public class ViewStationQueryHandler(IUnitOfWork Uow) : IRequestHandler<ViewStationQuery, PagedResult<StationDto>>
{
    public async Task<PagedResult<StationDto>> Handle(ViewStationQuery request, CancellationToken cancellationToken)
    {
        var query = Uow.Repository<Station>().AsQueryable();
        var totalItems = query.Count();
        var stations = await query
            .Skip((request.Paging.Page - 1) * request.Paging.PageSize)
            .Take(request.Paging.PageSize)
            .Select(station => new StationDto
            {
                Id = station.StationId,
                Name = station.Name,
                Address = station.Address
            })
            .ToListAsync();

        foreach (var station in stations)
        {
            var currentVehicles = await Uow.Repository<VehicleAtStation>().AsQueryable()
                .Where(vs => vs.StationId == station.Id && vs.EndTime == null)
                .Select(vs => vs.VehicleId)
                .ToListAsync();
            station.Capacity = currentVehicles.Count;
        }
        return new PagedResult<StationDto>
        (
            stations,
            request.Paging.Page,
            request.Paging.PageSize,
            totalItems
        );
    }
}
