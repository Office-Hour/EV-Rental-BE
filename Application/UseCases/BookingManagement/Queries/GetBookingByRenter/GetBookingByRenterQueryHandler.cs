using Application.CustomExceptions;
using Application.DTOs;
using Application.DTOs.BookingManagement;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.BookingManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.BookingManagement.Queries.GetBookingByRenter;

public class GetBookingByRenterQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<GetBookingByRenterQuery, PagedResult<BookingDetailsDto>>
{
    public async Task<PagedResult<BookingDetailsDto>> Handle(GetBookingByRenterQuery request, CancellationToken cancellationToken)
    {
        var renter = await uow.Repository<Renter>().GetByIdAsync(request.RenterId, cancellationToken) ?? throw new NotFoundException(errorMessage: "Renter not found");

        var query = uow.Repository<Booking>().AsQueryable().Where(b => b.RenterId == renter.RenterId);

        var totalItems = await query.CountAsync(cancellationToken: cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        var bookingDtos = mapper.Map<List<BookingDetailsDto>>(items);

        return new PagedResult<BookingDetailsDto>(
            Items: bookingDtos,
            Total: totalItems,
            Page: request.PageNumber,
            PageSize: request.PageSize
        );
    }
}
