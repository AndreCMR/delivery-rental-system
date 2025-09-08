using MediatR;

namespace delivery_rental_system.Application.Queries.Motorcycles;

public sealed record SearchMotorcyclesByPlateQuery(string? Plate)
    : IRequest<IReadOnlyList<MotorcycleDto>>;