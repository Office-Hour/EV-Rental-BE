using Application.DTOs;
using Application.DTOs.StationManagement;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.StationManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.StationManagement.Queries.FilterStation;

public class FilterStationQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<FilterStationQuery, PagedResult<StationDto>>
{
    public async Task<PagedResult<StationDto>> Handle(FilterStationQuery request, CancellationToken cancellationToken)
    {
        var query = uow.Repository<Station>().AsQueryable();

        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(s => s.Name.Contains(request.Name, StringComparison.CurrentCultureIgnoreCase));
        }
        if (!string.IsNullOrEmpty(request.Address))
        {
            query = query.Where(s => s.Address.Contains(request.Address, StringComparison.CurrentCultureIgnoreCase));
        }

        var totalRecords = await query.CountAsync(cancellationToken);
        var stations = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var stationDtos = mapper.Map<List<StationDto>>(stations);
        foreach (var station in stationDtos)
        {
            var currentVehicles = await uow.Repository<VehicleAtStation>().AsQueryable()
                .Where(vs => vs.StationId == station.Id && vs.EndTime == null)
                .Select(vs => vs.VehicleId)
                .ToListAsync(cancellationToken: cancellationToken);
            station.Capacity = currentVehicles.Count;
        }

        return new PagedResult<StationDto>(
            Items: stationDtos,
            Total: totalRecords,
            Page: request.PageNumber,
            PageSize: request.PageSize
        );
    }
}
