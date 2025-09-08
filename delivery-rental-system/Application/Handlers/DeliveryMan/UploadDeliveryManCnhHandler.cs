using delivery_rental_system.Application.Abstractions;
using delivery_rental_system.Application.Commands.Update;
using delivery_rental_system.Application.Helpers;
using MediatR;
using System;

namespace delivery_rental_system.Application.Handlers.DeliveryMan;

public sealed class UploadDeliveryManCnhHandler(
    IDeliveryManRepository repo,
    IFileStorage storage
) : IRequestHandler<UploadDeliveryManCnhCommand, Unit>
{
    public async Task<Unit> Handle(UploadDeliveryManCnhCommand req, CancellationToken ct)
    {
        var deliveryMan = await repo.GetByIdentifierAsync(req.Id, ct);
        if (deliveryMan is null)
            throw new InvalidOperationException("Dados inválidos"); 

        if (string.IsNullOrWhiteSpace(req.Base64))
            throw new InvalidOperationException("Dados inválidos");

        var (stream, fileName, contentType) = ImageBase64Helper.ToStreamPngOrBmp(req.Base64);

        if (!string.IsNullOrWhiteSpace(deliveryMan.CnhImagemUrl))
            await storage.DeleteIfExistsAsync(deliveryMan.CnhImagemUrl, ct);

        using (stream)
        {
            var url = await storage.SaveAsync(stream, fileName, contentType, ct);
            deliveryMan.UpdateCnhImageUrl(url); 
        }

        await repo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}