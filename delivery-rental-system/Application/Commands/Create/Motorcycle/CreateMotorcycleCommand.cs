using MediatR;

namespace delivery_rental_system.Application.Commands.Create;

public sealed record CreateMotorcycleCommand(
string Identifier,
int Year,
string Model,
string Plate) : IRequest<Unit>;
