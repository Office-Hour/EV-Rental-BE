using Application.DTOs.RentalManagement;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.RentalManagement.Queries.GetRentalFull;

public class GetRentalFullQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<GetRentalFullQuery, List<RentalDetailsDto>>
{
    public async Task<List<RentalDetailsDto>> Handle(GetRentalFullQuery request, CancellationToken cancellationToken)
    {
        var rentals = await uow.Repository<Domain.Entities.RentalManagement.Rental>()
            .AsQueryable()
            .ToListAsync(cancellationToken);
        return mapper.Map<List<RentalDetailsDto>>(rentals);
    }
}
