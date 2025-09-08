using delivery_rental_system.Domain.Entities;

namespace delivery_rental_system.Application.Abstractions;

public interface IDeliveryManRepository
{
    Task<bool> CnpjExistsAsync(string cnpj, CancellationToken ct);
    Task<bool> CnhExistsAsync(string cnh, CancellationToken ct);

    Task AddAsync(DeliveryMan entity, CancellationToken ct);
    Task<DeliveryMan?> GetByIdentifierAsync(string id, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
