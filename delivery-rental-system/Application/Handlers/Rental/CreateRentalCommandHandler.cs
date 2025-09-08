using delivery_rental_system.Application.Abstractions;
using delivery_rental_system.Application.Commands.Create.Rental;
using delivery_rental_system.Domain.Enums;
using MediatR;

namespace delivery_rental_system.Application.Handlers;

public sealed class CreateRentalCommandHandler(
    IDeliveryManRepository deliveryMen,
    IMotorcycleRepository motorcycles,
    IRentalRepository rentals
) : IRequestHandler<CreateRentalCommand, Unit>
{
    public async Task<Unit> Handle(CreateRentalCommand command, CancellationToken ct)
    {
        var deliveryMan = await deliveryMen.GetByIdentifierAsync(command.EntregadorId, ct);
        if (deliveryMan is null)
            throw new InvalidOperationException("Entregador não encontrado.");

        var motorcycle = await motorcycles.GetByIdentifierAsync(command.MotoId, ct);
        if (motorcycle is null)
            throw new InvalidOperationException("Moto não encontrada.");

        var rentalIdentifier = $"locacao{Random.Shared.Next(100, 999)}";

        var rental = new Domain.Entities.Rental.Rental(
               identifier: rentalIdentifier,
                deliveryManId: deliveryMan.Id,
                motorcycleId: motorcycle.Id,
                plan: (RentalPlan)command.Plano,
                dataInicio: command.DataInicio,
                dataTermino: command.DataTermino,
                dataPrevisaoTermino: command.DataPrevisaoTermino,
                nowUtc: DateTime.UtcNow
        );

        await rentals.AddAsync(rental, ct);

        try{
            await rentals.SaveChangesAsync(ct);

        }
        catch (Exception ex)
        {

        }
        return Unit.Value;
    }
}