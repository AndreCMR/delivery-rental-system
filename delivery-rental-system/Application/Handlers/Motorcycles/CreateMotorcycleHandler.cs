using delivery_rental_system.Application.Abstractions;
using delivery_rental_system.Application.Commands.Create;
using delivery_rental_system.Domain.Entities;
using delivery_rental_system.Domain.Events;
using MassTransit;
using MediatR;

namespace delivery_rental_system.Application.Handlers.Motorcycles;
public sealed class CreateMotorcycleHandler(IMotorcycleRepository _repo, IPublishEndpoint _publisher) : IRequestHandler<CreateMotorcycleCommand, Unit>
{
    public async Task<Unit> Handle(CreateMotorcycleCommand request, CancellationToken ct)
    {
        if (await _repo.PlateExistsAsync(request.Plate, ct))
            throw new InvalidOperationException("Dados inválidos");

        var entity = new Motorcycle(request.Identifier, request.Year,
        request.Model, request.Plate);

        await _repo.AddAsync(entity, ct);

        await _repo.SaveChangesAsync(ct);

        await _publisher.Publish(new MotorcycleCreated(entity.Identifier,entity.Year,
            entity.Model, entity.Plate, DateTime.UtcNow), ct);

        return Unit.Value;
    }
}
