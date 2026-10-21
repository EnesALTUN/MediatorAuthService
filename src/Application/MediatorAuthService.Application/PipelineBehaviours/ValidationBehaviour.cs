using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace MediatorAuthService.Application.PipelineBehaviours;

public class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next(cancellationToken);

        ValidationContext<TRequest> context = new(request);

        ValidationResult[] results = await Task.WhenAll(_validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        List<ValidationFailure> failures = [.. results.SelectMany(result => result.Errors).Where(failure => failure is not null)];

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next(cancellationToken);
    }
}
