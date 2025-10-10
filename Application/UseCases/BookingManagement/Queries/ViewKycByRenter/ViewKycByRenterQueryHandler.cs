using Application.DTOs;
using Application.DTOs.BookingManagement;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.BookingManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.BookingManagement.Queries.ViewKycByRenter;

public class ViewKycByRenterQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<ViewKycByRenterQuery, PagedResult<KycDto>>
{
    public async Task<PagedResult<KycDto>> Handle(ViewKycByRenterQuery request, CancellationToken cancellationToken)
    {
        var kycRepo = uow.Repository<Kyc>();
        var query = kycRepo.AsQueryable().Where(k => k.RenterId == request.RenterId);
        var totalItems = await query.CountAsync(cancellationToken: cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        var dtos = mapper.Map<List<KycDto>>(items);

        return new PagedResult<KycDto>(dtos, totalItems, request.PageNumber, request.PageSize);
    }
}
