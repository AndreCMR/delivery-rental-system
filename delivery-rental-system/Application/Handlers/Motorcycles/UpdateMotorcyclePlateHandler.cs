using delivery_rental_system.Application.Commands;
using delivery_rental_system.Infra.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace delivery_rental_system.Application.Handlers.Motorcycles;

public sealed class UpdateMotorcyclePlateHandler(AppDbContext _context) : IRequestHandler<UpdateMotorcyclePlateCommand, Unit>
{
    public async Task<Unit> Handle(UpdateMotorcyclePlateCommand request, CancellationToken cancellationToken)
    {
        var motorcycle = await _context.Motorcycles
            .FirstOrDefaultAsync(m => m.Identifier == request.Id, cancellationToken);

        if (motorcycle is null)
                throw new InvalidOperationException("Dados inválidos");

        var existsPlate = await _context.Motorcycles
            .AnyAsync(m => m.Plate == request.Plate && m.Identifier != request.Id, cancellationToken);

        if (existsPlate)
            throw new InvalidOperationException("Dados inválidos");
    
        var result = motorcycle.UpdatePlate(request.Plate);

        if (result)
        {
            _context.Motorcycles.Update(motorcycle);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}