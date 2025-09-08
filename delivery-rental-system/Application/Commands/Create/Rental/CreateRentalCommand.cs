using MediatR;

namespace delivery_rental_system.Application.Commands.Create.Rental;

public sealed record CreateRentalCommand(string EntregadorId,
    string MotoId,
    DateTime DataInicio,
    DateTime DataTermino,
    DateTime DataPrevisaoTermino,
    int Plano) : IRequest<Unit>;
