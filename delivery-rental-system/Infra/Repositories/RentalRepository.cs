using delivery_rental_system.Application.Abstractions;
using delivery_rental_system.Domain.Entities;
using delivery_rental_system.Domain.Entities.Rental;
using delivery_rental_system.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace delivery_rental_system.Infra.Repositories;

public sealed class RentalRepository(AppDbContext db) : IRentalRepository
{
    public Task<bool> ExistsByMotorcycleAsync(string motorcycleId, CancellationToken ct)
        => db.Rental.Include(x=> x.Motorcycle).AnyAsync(r => r.Motorcycle.Identifier == motorcycleId, ct);

    public Task<bool> MotorcycleHasOverlappingActiveRentalAsync(
       string motorcycleId, DateTime start, DateTime predictedEnd, CancellationToken ct)
       => db.Rental.Include(x => x.Motorcycle).AnyAsync(r =>
              r.Motorcycle.Identifier == motorcycleId
           && r.Active
           && r.StartDate <= predictedEnd
           && (r.EndDate ?? r.PredictedEndDate) >= start, ct);

    public Task AddAsync(Rental rental, CancellationToken ct) => db.Rental.AddAsync(rental, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public async Task<Rental?> GetByIdentifierAsync(string identifier, CancellationToken ct) 
        => await db.Rental.FirstOrDefaultAsync(x => x.Identifier == identifier, ct);
}
