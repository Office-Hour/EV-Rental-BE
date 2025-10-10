using Application.DTOs;
using Application.DTOs.StationManagement;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.StationManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Application.UseCases.StationManagement.Queries.ViewStation;

public class ViewStationQueryHandler(IUnitOfWork Uow, IMapper mapper) : IRequestHandler<ViewStationQuery, PagedResult<StationDto>>
{
    public async Task<PagedResult<StationDto>> Handle(ViewStationQuery request, CancellationToken cancellationToken)
    {
        var query = Uow.Repository<Station>().AsQueryable();
        var totalItems = query.Count();
        var stations = await query
            .Skip((request.Paging.Page - 1) * request.Paging.PageSize)
            .Take(request.Paging.PageSize)
            .ToListAsync();
        var stationDtos = mapper.Map<List<StationDto>>(stations);
        foreach (var station in stationDtos)
        {
            var currentVehicles = await Uow.Repository<VehicleAtStation>().AsQueryable()
                .Where(vs => vs.StationId == station.Id && vs.EndTime == null)
                .Select(vs => vs.VehicleId)
                .ToListAsync(cancellationToken: cancellationToken);
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
