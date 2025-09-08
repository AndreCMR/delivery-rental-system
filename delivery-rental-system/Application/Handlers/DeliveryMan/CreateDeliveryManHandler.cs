using delivery_rental_system.Application.Abstractions;
using delivery_rental_system.Application.Commands.Create.DeliveryMan;
using delivery_rental_system.Domain.Enums;
using MediatR;

namespace delivery_rental_system.Application.Handlers.DeliveryMan;

public sealed class CreateDeliveryManHandler(IDeliveryManRepository repo, IFileStorage storage) : IRequestHandler<CreateDeliveryManCommand, Unit>
{
    public async Task<Unit> Handle(CreateDeliveryManCommand req, CancellationToken ct)
    {
        var cnpj = OnlyDigits(req.Cnpj);
        var cnh = OnlyDigits(req.NumeroCnh);

        CnhEnumConverter.TryParse(req.TipoCnh, out var cnhEnum);


        string? cnhUrl = null;

        if (!string.IsNullOrWhiteSpace(req.ImagemCnh))
        {
            var (stream, fileName, contentType) = ToStreamPngOrBmp(req.ImagemCnh);
            using (stream)
                cnhUrl = await storage.SaveAsync(stream, fileName, contentType, ct);
        }

        var entity = new Domain.Entities.DeliveryMan(
            identifier: req.Identificador.Trim(),
            nome: req.Nome.Trim(),
            cnpj: cnpj,
            dataNascimento: req.DataNascimento,
            cnhNumero: cnh,
            cnhEnum: cnhEnum,
            cnhImagemUrl: cnhUrl
        );

        await repo.AddAsync(entity, ct);

        await repo.SaveChangesAsync(ct);

        return Unit.Value; 
    }

    private static string OnlyDigits(string s) =>
        new string((s ?? string.Empty).Where(char.IsDigit).ToArray());

    private static (Stream stream, string fileName, string contentType) ToStreamPngOrBmp(string input)
    {
        byte[] bytes;
        string mime, ext;

        if (input.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            var i = input.IndexOf(";base64,", StringComparison.OrdinalIgnoreCase);
            mime = input[5..i]; 
            bytes = Convert.FromBase64String(input[(i + 8)..]);
            ext = mime == "image/png" ? ".png" : ".bmp";
        }
        else
        {
            bytes = Convert.FromBase64String(input);
            var isPng = bytes.Length >= 8 &&
                        bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                        bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;
            mime = isPng ? "image/png" : "image/bmp";
            ext = isPng ? ".png" : ".bmp";
        }

        var ms = new MemoryStream(bytes);
        var name = $"cnh_{Guid.NewGuid():N}{ext}";
        return (ms, name, mime);
    }
}
