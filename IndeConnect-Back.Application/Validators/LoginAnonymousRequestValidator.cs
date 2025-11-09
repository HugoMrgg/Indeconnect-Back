using FluentValidation;
using IndeConnect_Back.Application.DTOs.Auth;

namespace IndeConnect_Back.Application.Validators;

public class LoginAnonymousRequestValidator : AbstractValidator<LoginAnonymousRequest>
{
    public LoginAnonymousRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}