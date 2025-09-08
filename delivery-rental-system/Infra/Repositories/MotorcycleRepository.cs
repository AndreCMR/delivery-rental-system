using delivery_rental_system.Application.Abstractions;
using delivery_rental_system.Domain.Entities;
using delivery_rental_system.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace delivery_rental_system.Infra.Repositories;

public sealed class MotorcycleRepository(AppDbContext _context) : IMotorcycleRepository
{
    public Task<bool> PlateExistsAsync(string plate, CancellationToken ct)
        => _context.Motorcycles.AnyAsync(x => x.Plate == plate, ct);

    public Task<bool> ExistsAsync(string id, CancellationToken ct)
    => _context.Motorcycles.AnyAsync(x => x.Identifier == id, ct);
    public Task AddAsync(Motorcycle entity, CancellationToken ct)
        => _context.Motorcycles.AddAsync(entity, ct).AsTask();

    public Task<int> SaveChangesAsync(CancellationToken ct)
        => _context.SaveChangesAsync(ct);

    public async Task<Motorcycle?> GetByIdentifierAsync(string id, CancellationToken ct) 
        =>  await _context.Motorcycles.FirstOrDefaultAsync(x => x.Identifier == id, ct);
    
}

