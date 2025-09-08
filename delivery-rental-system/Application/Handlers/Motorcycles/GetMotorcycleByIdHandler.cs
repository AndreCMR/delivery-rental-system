using delivery_rental_system.Application.Queries.Motorcycles;
using delivery_rental_system.Infra.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace delivery_rental_system.Application.Handlers.Motorcycles;

public sealed class GetMotorcycleByIdHandler(AppDbContext _context) : IRequestHandler<GetMotorcycleByIdQuery, MotorcycleDto?>
{
    public async Task<MotorcycleDto?> Handle(GetMotorcycleByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Motorcycles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Identifier == request.Id, ct);

        return e is null
            ? null
            : new MotorcycleDto(e.Identifier, e.Year, e.Model, e.Plate, e.CreatedAt);
    }
}