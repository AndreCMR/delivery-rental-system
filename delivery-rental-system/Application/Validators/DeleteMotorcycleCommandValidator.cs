namespace delivery_rental_system.Application.Validators;

using delivery_rental_system.Application.Abstractions;
using delivery_rental_system.Application.Commands.Delete;
using FluentValidation;

public sealed class DeleteMotorcycleCommandValidator : AbstractValidator<DeleteMotorcycleCommand>
{
    public DeleteMotorcycleCommandValidator(
        IMotorcycleRepository motorcycles,
        IRentalRepository rentals)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id da moto é obrigatório.")
            .MustAsync(async (id, ct) => await motorcycles.ExistsAsync(id, ct))
            .WithMessage("Moto não encontrada.");

        RuleFor(x => x.Id)
            .MustAsync(async (id, ct) => !await rentals.ExistsByMotorcycleAsync(id, ct))
            .WithMessage("Moto não pode ser removida: há locações associadas.");
    }
}

