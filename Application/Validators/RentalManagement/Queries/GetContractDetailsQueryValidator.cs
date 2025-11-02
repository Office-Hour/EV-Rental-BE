using Application.UseCases.RentalManagement.Queries.GetContractDetails;
using FluentValidation;

namespace Application.Validators.RentalManagement.Queries;

public class GetContractDetailsQueryValidator : AbstractValidator<GetContractDetailsQuery>
{
    public GetContractDetailsQueryValidator()
    {
        RuleFor(x => x.ContractId)
            .NotEmpty().WithMessage("ContractId is required.");
    }
}
