using delivery_rental_system.Presentation.Responses;
using MediatR;

namespace delivery_rental_system.Application.Commands.Delete;

public sealed record DeleteMotorcycleCommand(string Id) : IRequest<ApiMessageResponse>;
