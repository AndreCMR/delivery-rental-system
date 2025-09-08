using delivery_rental_system.Application.Queries.Motorcycles;
using delivery_rental_system.Infra.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace delivery_rental_system.Application.Handlers.Motorcycles;

public sealed class SearchMotorcyclesByPlateHandler(AppDbContext _context) : IRequestHandler<SearchMotorcyclesByPlateQuery, IReadOnlyList<MotorcycleDto>>
{
    public async Task<IReadOnlyList<MotorcycleDto>> Handle(SearchMotorcyclesByPlateQuery request, CancellationToken ct)
    {
        var q = _context.Motorcycles.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Plate))
            q = q.Where(x => x.Plate == request.Plate);

        var list = await q
            .OrderByDescending(x => x.CreatedAt)
            .Select(e => new MotorcycleDto(e.Identifier, e.Year, e.Model, e.Plate, e.CreatedAt))
            .ToListAsync(ct);

        return list;
    }
}