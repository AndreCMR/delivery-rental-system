using delivery_rental_system.Application.Abstractions;
using delivery_rental_system.Application.Commands.Create.Rental;
using delivery_rental_system.Domain.Enums;
using FluentValidation;

namespace delivery_rental_system.Application.Validators;
public sealed class CreateRentalCommandValidator : AbstractValidator<CreateRentalCommand>
{
    public CreateRentalCommandValidator(
        IDeliveryManRepository deliveryMen,
        IMotorcycleRepository motorcycles,
        IRentalRepository rentals)
    {
        RuleFor(x => x.EntregadorId)
            .NotEmpty().WithMessage("EntregadorId é obrigatório.");

        RuleFor(x => x.MotoId)
            .NotEmpty().WithMessage("MotoId é obrigatório.");

        RuleFor(x => x.Plano)
            .Must(p => p is 7 or 15 or 30 or 45 or 50)
            .WithMessage("Plano inválido.");

        RuleFor(x => x.DataInicio).NotEmpty();
        RuleFor(x => x.DataPrevisaoTermino).NotEmpty();
        RuleFor(x => x.DataTermino).NotEmpty();

        RuleFor(x => x).CustomAsync(async (cmd, ctx, ct) =>
        {
            var hasError = false;

            var deliveryMan = await deliveryMen.GetByIdentifierAsync(cmd.EntregadorId, ct);

            if (deliveryMan is null)
            {
                ctx.AddFailure("entregador_id", "Entregador não encontrado.");
                hasError = true;
            }
            else if (deliveryMan.CnhEnum is not CnhEnum.A)
            {
                ctx.AddFailure("entregador_id", "Entregador não habilitado na categoria A.");
                hasError = true;
            }

            var moto = await motorcycles.GetByIdentifierAsync(cmd.MotoId, ct);
            if (moto is null)
            {
                ctx.AddFailure("moto_id", "Moto não encontrada.");
                hasError = true;
            }

            var hojeUtc = DateTime.UtcNow.Date;
            var esperadoInicio = hojeUtc.AddDays(1);

        
            if (cmd.DataInicio.Date != esperadoInicio)
            {
                ctx.AddFailure("data_inicio", "Deve ser o dia seguinte à criação (UTC).");
                hasError = true;
            }

            if (!hasError)
            {
                var hasOverlap = await rentals.MotorcycleHasOverlappingActiveRentalAsync(moto.Identifier, cmd.DataInicio, cmd.DataPrevisaoTermino, ct);

                if (hasOverlap)
                    ctx.AddFailure("moto_id", "Moto indisponível no período.");
            }
        });
    }
}
