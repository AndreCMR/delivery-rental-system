using delivery_rental_system.Application.Commands.Delete;
using delivery_rental_system.Infra.Persistence;
using delivery_rental_system.Presentation.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace delivery_rental_system.Application.Handlers.Motorcycles;
public sealed class DeleteMotorcycleHandler( AppDbContext _context) : IRequestHandler<DeleteMotorcycleCommand, ApiMessageResponse>
{
    public async Task<ApiMessageResponse> Handle(DeleteMotorcycleCommand request, CancellationToken cancellationToken)
    {
        var motorcycle = await _context.Motorcycles
            .FirstOrDefaultAsync(m => m.Identifier == request.Id, cancellationToken);

        if (motorcycle is null)
            return new ApiMessageResponse("Dados inválidos");

        _context.Motorcycles.Remove(motorcycle);
        await _context.SaveChangesAsync(cancellationToken);

        return null!;
    }
}
