using Application.CustomExceptions;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.RentalManagement;
using Domain.Enums;
using MediatR;

namespace Application.UseCases.RentalManagement.Commands.SignContract;

public class SignContractCommandHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<SignContractCommand, Guid>
{
    public async Task<Guid> Handle(SignContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await uow.Repository<Contract>()
            .GetByIdAsync(request.CreateSignaturePayloadDto.ContractId, cancellationToken)
            ?? throw new NotFoundException("Contract not found");

        var rental = await uow.Repository<Rental>()
            .GetByIdAsync(contract.RentalId, cancellationToken)
            ?? throw new NotFoundException("Rental not found");

        var inspections = uow.Repository<Inspection>()
            .AsQueryable()
            .Where(i => i.RentalId == rental.RentalId)
            .ToList();
        if (inspections.Count == 0)
        {
            throw new InvalidOperationException("Rental must have at least one inspection before signing the contract.");
        }

        if (contract.Status == ContractStatus.Issued)
        {
            contract.Status = ContractStatus.Partially_Signed;
            contract.CompletedAt = DateTime.UtcNow;
        }
        else if (contract.Status == ContractStatus.Partially_Signed)
        {
            contract.Status = ContractStatus.Signed;
            contract.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            throw new InvalidOperationException("Contract is already signed");
        }

        var newSignature = mapper.Map<Signature>(request.CreateSignaturePayloadDto);
        newSignature.SignatureId = Guid.NewGuid();

        if (request.ESignPayload != null)
        {
            newSignature.SignerIp = request.ESignPayload.SignerIp;
            newSignature.UserAgent = request.ESignPayload.UserAgent;
            newSignature.ProviderSignatureId = request.ESignPayload.ProviderSignatureId;
            newSignature.SignatureImageUrl = request.ESignPayload.SignatureImageUrl;
            newSignature.CertSubject = request.ESignPayload.CertSubject;
            newSignature.CertIssuer = request.ESignPayload.CertIssuer;
            newSignature.CertSerial = request.ESignPayload.CertSerial;
            newSignature.CertFingerprintSha256 = request.ESignPayload.CertFingerprintSha256;
            newSignature.SignatureHash = request.ESignPayload.SignatureHash;
            newSignature.EvidenceUrl = request.ESignPayload.EvidenceUrl;
        }

        await uow.Repository<Contract>().UpdateAsync(contract.ContractId, contract, cancellationToken);
        await uow.Repository<Signature>().AddAsync(newSignature, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        return contract.ContractId;
    }
}
