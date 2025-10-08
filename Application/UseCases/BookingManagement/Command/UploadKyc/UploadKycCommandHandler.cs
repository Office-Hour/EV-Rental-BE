using Application.CustomExceptions;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.BookingManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.BookingManagement.Command.UploadKyc;

public class UploadKycCommandHandler(IUnitOfWork Uow, IMapper mapper) : IRequestHandler<UploadKycCommand>
{
    public async Task Handle(UploadKycCommand request, CancellationToken cancellationToken)
    {
        var kyc = mapper.Map<Kyc>(request);
        var renter = await Uow.Repository<Renter>()
            .AsQueryable()
            .FirstOrDefaultAsync(r => r.UserId == request.UserId, cancellationToken: cancellationToken);
        if (renter == null)
        {
            throw new InvalidTokenException("Renter not found");
        }
        kyc.RenterId = renter.RenterId;
        await Uow.Repository<Kyc>().AddAsync(kyc, cancellationToken);
        await Uow.SaveChangesAsync(cancellationToken);
    }
}
