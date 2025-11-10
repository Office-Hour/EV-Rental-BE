using Application.CustomExceptions;
using Application.DTOs.BookingManagement;
using Application.DTOs.RentalManagement;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.RentalManagement;
using Domain.Entities.StationManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.RentalManagement.Queries.GetRentalDetails;

public class GetRentalDetailsQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<GetRentalDetailsQuery, RentalDetailsDto>
{
    public async Task<RentalDetailsDto> Handle(GetRentalDetailsQuery request, CancellationToken cancellationToken)
    {
        var rental = await uow.Repository<Rental>()
            .GetByIdAsync(request.RentalId, cancellationToken)
            ?? throw new NotFoundException("Rental not found");

        var booking = await uow.Repository<Domain.Entities.BookingManagement.Booking>()
            .GetByIdAsync(rental.BookingId, cancellationToken)
            ?? throw new NotFoundException("Booking not found");

        var vehicle = await uow.Repository<VehicleAtStation>().AsQueryable()
            .Where(v => v.VehicleId == rental.VehicleId && v.EndTime == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Vehicle not found");

        var rentalDetailsDto = mapper.Map<RentalDetailsDto>(rental);
        rentalDetailsDto.Booking = mapper.Map<BookingBriefDto>(booking);
        rentalDetailsDto.Vehicle = mapper.Map<VehicleDto>(vehicle);

        var contracts = uow.Repository<Contract>()
            .AsQueryable()
            .Where(c => c.RentalId == rental.RentalId)
            .ToList();

        rentalDetailsDto.Contracts = mapper.Map<List<ContractDto>>(contracts);

        return rentalDetailsDto;
    }
}
