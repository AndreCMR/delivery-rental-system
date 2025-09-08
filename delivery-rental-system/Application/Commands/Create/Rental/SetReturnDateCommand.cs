using MediatR;

namespace delivery_rental_system.Application.Commands.Create.Rental;

public sealed record SetReturnDateCommand(string RentalId, DateTime ReturnDateUtc) : IRequest<Unit>;
