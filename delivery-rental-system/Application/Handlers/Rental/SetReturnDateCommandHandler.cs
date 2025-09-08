using delivery_rental_system.Application.Abstractions;
using delivery_rental_system.Application.Commands.Create.Rental;
using delivery_rental_system.Domain.Enums;
using MediatR;

namespace delivery_rental_system.Application.Handlers.Rental;
public sealed class SetReturnDateCommandHandler(IRentalRepository repo)
    : IRequestHandler<SetReturnDateCommand, Unit>
{
    public async Task<Unit> Handle(SetReturnDateCommand request, CancellationToken ct)
    {
        var rental = await repo.GetByIdentifierAsync(request.RentalId, ct);

        if (rental is null)
            throw new InvalidOperationException("Dados inválidos");

        if (!rental.Active)
            throw new InvalidOperationException("Dados inválidos");

        var startDate = rental.StartDate.Date;
        var predictedEndDate = rental.PredictedEndDate.Date;
        var returnDate = request.ReturnDateUtc.Date;

        if (returnDate < startDate)
            throw new InvalidOperationException("Dados inválidos");

        var planDays = rental.Plan.GetDays();
        var dailyRate = rental.Plan.GetDailyRate();

        var usedDays = (returnDate - startDate).Days + 1;

        decimal penalty = 0m;
        decimal extraValue = 0m;
        decimal totalValue;

        if (returnDate < predictedEndDate)
        {
            var notUsedDays = Math.Max(0, planDays - usedDays);
            var baseValue = usedDays * dailyRate;

            penalty = rental.Plan switch
            {
                RentalPlan.Days7 => notUsedDays * dailyRate * 0.20m,
                RentalPlan.Days15 => notUsedDays * dailyRate * 0.40m,
                _ => 0m
            };

            totalValue = baseValue + penalty;
        }
        else if (returnDate > predictedEndDate)
        {
            var extraDays = (returnDate - predictedEndDate).Days;
            extraValue = extraDays * 50m;
            totalValue = planDays * dailyRate + extraValue;
        }
        else
        {
            totalValue = planDays * dailyRate;
        }

        rental.SetReturnDate(returnDate);
        rental.SetRentalValue(totalValue);

        await repo.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
