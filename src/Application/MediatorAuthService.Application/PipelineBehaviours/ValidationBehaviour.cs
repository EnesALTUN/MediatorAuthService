using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace MediatorAuthService.Application.PipelineBehaviours;

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ValidationContext<TRequest> context = new(request);
        List<ValidationFailure> failures = _validators.Select(x => x.Validate(context))
                                  .SelectMany(x => x.Errors)
                                  .Where(x => x != null)
                                  .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return next();
    }
}