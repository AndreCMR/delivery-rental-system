using MediatR;

namespace delivery_rental_system.Application.Queries.Motorcycles;

public sealed record GetMotorcycleByIdQuery(string Id)
    : IRequest<MotorcycleDto?>;

public sealed record MotorcycleDto(
    string Identifier,
    int Year,
    string Model,
    string Plate,
    DateTime CreatedAtUtc);