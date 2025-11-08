using FluentValidation;
using IndeConnect_Back.Application.DTOs.Subscriptions;

namespace IndeConnect_Back.Application.Validators;

public class CreateBrandSubscriptionRequestValidator : AbstractValidator<CreateBrandSubscriptionRequest>
{
    public CreateBrandSubscriptionRequestValidator()
    {
        RuleFor(x => x.BrandId)
            .NotEmpty().WithMessage("Brand ID is required")
            .GreaterThan(0).WithMessage("Brand ID must be greater than 0");
    }
}