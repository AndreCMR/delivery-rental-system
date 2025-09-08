using MediatR;

namespace delivery_rental_system.Application.Commands.Update;

public sealed record UploadDeliveryManCnhCommand(string Id, string Base64) : IRequest<Unit>;