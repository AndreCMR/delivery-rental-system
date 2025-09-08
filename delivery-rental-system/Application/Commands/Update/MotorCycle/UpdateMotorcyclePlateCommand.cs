using MediatR;

namespace delivery_rental_system.Application.Commands;

public sealed record UpdateMotorcyclePlateCommand(string Id, string Plate) : IRequest<Unit>;

