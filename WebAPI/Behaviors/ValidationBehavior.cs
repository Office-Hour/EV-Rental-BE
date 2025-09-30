using FluentValidation;
using MediatR;

namespace WebAPI.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest> validator)
        : IPipelineBehavior<TRequest, TResponse> where TRequest : class, IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (validator == null)
            {
                return await next(cancellationToken);
            }

            var context = new ValidationContext<TRequest>(request);

            var errorsDictionary = validator.Validate(context).Errors;

            if (errorsDictionary.Count > 0)
            {
                throw new ValidationException(errorsDictionary);
            }

            return await next(cancellationToken);
        }
    }
}
