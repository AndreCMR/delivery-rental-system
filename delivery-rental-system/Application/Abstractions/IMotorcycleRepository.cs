using delivery_rental_system.Domain.Entities;

namespace delivery_rental_system.Application.Abstractions;

public interface IMotorcycleRepository
{
    Task<bool> PlateExistsAsync(string plate, CancellationToken ct);
    Task AddAsync(Motorcycle entity, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);

    Task<bool> ExistsAsync(string id, CancellationToken ct);

    Task<Motorcycle?> GetByIdentifierAsync(string id, CancellationToken ct);
}