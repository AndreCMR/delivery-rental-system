using delivery_rental_system.Domain.Entities.Rental;

namespace delivery_rental_system.Application.Abstractions;

public interface IRentalRepository
{
    Task<bool> ExistsByMotorcycleAsync(string motorcycleId, CancellationToken ct);

    Task<bool> MotorcycleHasOverlappingActiveRentalAsync(
           string motorcycleId, DateTime start, DateTime predictedEnd, CancellationToken ct);
    
    Task AddAsync(Rental rental, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);

    Task<Rental?> GetByIdentifierAsync(string identifier, CancellationToken ct);
}