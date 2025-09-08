using delivery_rental_system.Application.Abstractions;
using delivery_rental_system.Domain.Entities;
using delivery_rental_system.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace delivery_rental_system.Infra.Repositories;

public sealed class DeliveryManRepository(AppDbContext db) : IDeliveryManRepository
{
    public async Task<bool> CnpjExistsAsync(string cnpj, CancellationToken ct)
    {
        return await db.DeliveryMan.AsNoTracking()
            .AnyAsync(x => x.Cnpj == cnpj, ct);
    }

    public async Task<bool> CnhExistsAsync(string cnh, CancellationToken ct)
    {

        return await db.DeliveryMan.AsNoTracking()
            .AnyAsync(x => x.CnhNumero == cnh, ct);
    }

    public async Task AddAsync(DeliveryMan entity, CancellationToken ct)
    {
        await db.DeliveryMan.AddAsync(entity, ct);
    }

    public async Task<DeliveryMan?> GetByIdentifierAsync(string id, CancellationToken ct)
    {
        return await db.DeliveryMan
            .FirstOrDefaultAsync(x => x.Identifier == id, ct);
    }     

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

}
