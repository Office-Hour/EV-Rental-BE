using Application.UseCases.RentalManagement.Commands.SignContract;
using FluentValidation;
using Application.DTOs.RentalManagement;
using Domain.Enums;

namespace Application.Validators.RentalManagement.Commands;

public class SignContractCommandValidator : AbstractValidator<SignContractCommand>
{
    public SignContractCommandValidator()
    {
        RuleFor(x => x.CreateSignaturePayloadDto)
            .NotNull().WithMessage("Signature payload is required.");

        When(x => x.CreateSignaturePayloadDto != null, () =>
        {
            RuleFor(x => x.CreateSignaturePayloadDto.ContractId)
                .NotEmpty().WithMessage("ContractId is required.");

            RuleFor(x => x.CreateSignaturePayloadDto.DocumentUrl)
                .NotEmpty().WithMessage("DocumentUrl is required.");

            RuleFor(x => x.CreateSignaturePayloadDto.DocumentHash)
                .NotEmpty().WithMessage("DocumentHash is required.");

            RuleFor(x => x.CreateSignaturePayloadDto.Role)
                .IsInEnum().WithMessage("Invalid party role.");

            RuleFor(x => x.CreateSignaturePayloadDto.SignatureEvent)
                .IsInEnum().WithMessage("Invalid signature event.");

            RuleFor(x => x.CreateSignaturePayloadDto.Type)
                .IsInEnum().WithMessage("Invalid signature type.");

            RuleFor(x => x.CreateSignaturePayloadDto.SignedAt)
                .NotEmpty().WithMessage("SignedAt is required.");
        });

        // If ESignPayload is provided, validate its fields
        When(x => x.ESignPayload != null, () =>
        {
            RuleFor(x => x.ESignPayload!.SignerIp)
                .NotEmpty().WithMessage("SignerIp is required for e-signature.");

            RuleFor(x => x.ESignPayload!.UserAgent)
                .NotEmpty().WithMessage("UserAgent is required for e-signature.");

            RuleFor(x => x.ESignPayload!.ProviderSignatureId)
                .NotEmpty().WithMessage("ProviderSignatureId is required for e-signature.");

            RuleFor(x => x.ESignPayload!.SignatureImageUrl)
                .NotEmpty().WithMessage("SignatureImageUrl is required for e-signature.");

            RuleFor(x => x.ESignPayload!.SignatureHash)
                .NotEmpty().WithMessage("SignatureHash is required for e-signature.");
        });
    }
}