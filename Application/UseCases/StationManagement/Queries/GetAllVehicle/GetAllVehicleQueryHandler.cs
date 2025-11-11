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
        var query = vehicleRepo.AsQueryable();
        var vehicles = await query.ToListAsync(cancellationToken);
        var dtos = mapper.Map<List<VehicleDetailsDto>>(vehicles);
        return dtos;
    }
}
