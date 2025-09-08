using delivery_rental_system.Application.Queries;
using delivery_rental_system.Infra.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace delivery_rental_system.Application.Handlers.Rental;

public sealed class GetRentalByIdHandler(AppDbContext context)
   : IRequestHandler<GetRentalByIdQuery, RentalDto?>
{
    public async Task<RentalDto?> Handle(GetRentalByIdQuery request, CancellationToken ct)
    {
        try
        {


            var rentalEntity = await context.Rental
                .AsNoTracking()
                .FirstOrDefaultAsync(rental => rental.Identifier == request.Id, ct);


            if (rentalEntity is null) return null;

            var deliveryManEntity = await context.DeliveryMan
                .AsNoTracking()
                .FirstOrDefaultAsync(deliveryMan => deliveryMan.Id == rentalEntity.DeliveryManId, ct);

            var motorcycleEntity = await context.Motorcycles
                .AsNoTracking()
                .FirstOrDefaultAsync(motorcycle => motorcycle.Id == rentalEntity.MotorcycleId, ct);

            if (deliveryManEntity is null || motorcycleEntity is null)
                return null;

            return new RentalDto(
                Identificador: rentalEntity.Identifier,
                ValorDiaria: rentalEntity.RentalValue,
                EntregadorId: deliveryManEntity.Identifier,
                MotoId: motorcycleEntity.Identifier,
                DataInicio: rentalEntity.StartDate,
                DataTermino: rentalEntity.EndDate ?? rentalEntity.PredictedEndDate,
                DataPrevisaoTermino: rentalEntity.PredictedEndDate,
                DataDevolucao: rentalEntity.EndDate
            );
        }
        catch (Exception ex)
        {

        }

        return null;
    }
}
