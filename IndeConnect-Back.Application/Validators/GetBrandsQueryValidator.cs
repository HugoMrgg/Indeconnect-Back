using FluentValidation;
using IndeConnect_Back.Application.DTOs.Brands;

namespace IndeConnect_Back.Application.Validators;

public class GetBrandsQueryValidator : AbstractValidator<GetBrandsQuery>
{
    public GetBrandsQueryValidator()
    {
        RuleFor(x => x.SortBy)
            .IsInEnum()
            .WithMessage("Invalid sort type");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        // Validation des coordonnées GPS (si fournies)
        When(x => x.Latitude.HasValue || x.Longitude.HasValue, () =>
        {
            RuleFor(x => x.Latitude)
                .NotNull()
                .InclusiveBetween(-90, 90)
                .WithMessage("Latitude must be between -90 and 90");

            RuleFor(x => x.Longitude)
                .NotNull()
                .InclusiveBetween(-180, 180)
                .WithMessage("Longitude must be between -180 and 180");
        });
    }
}