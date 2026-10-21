using FluentValidation;
using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Domain.Core.Pagination;

namespace MediatorAuthService.Application.Validators.UserValidators;

public class GetUserAllValidator : AbstractValidator<GetUserAllQuery>
{
    public GetUserAllValidator()
    {
        RuleFor(x => x.PageId)
            .InclusiveBetween(1, 1_000_000);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PaginationParams.MaxPageSize);

        RuleFor(x => x.OrderType)
            .Must(orderType => string.Equals(orderType, "ascending", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(orderType, "descending", StringComparison.OrdinalIgnoreCase))
            .When(x => !string.IsNullOrEmpty(x.OrderType))
            .WithMessage("OrderType must be 'ascending' or 'descending'.");
    }
}
