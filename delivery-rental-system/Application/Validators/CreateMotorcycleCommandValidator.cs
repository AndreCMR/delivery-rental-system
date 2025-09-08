using delivery_rental_system.Application.Commands.Create;
using FluentValidation;

namespace delivery_rental_system.Application.Validators;

public sealed class CreateMotorcycleCommandValidator : AbstractValidator<CreateMotorcycleCommand>
{
    public CreateMotorcycleCommandValidator()
    {
        RuleFor(x => x.Year)
        .InclusiveBetween(1900, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.Model)
        .NotEmpty()
        .MaximumLength(120);
        RuleFor(x => x.Plate)
        .NotEmpty()
        .MaximumLength(16);
    }
}


