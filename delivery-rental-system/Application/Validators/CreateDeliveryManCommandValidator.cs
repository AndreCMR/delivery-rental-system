using delivery_rental_system.Application.Abstractions;
using delivery_rental_system.Application.Commands.Create.DeliveryMan;
using delivery_rental_system.Domain.Enums;
using FluentValidation;

namespace delivery_rental_system.Application.Validators;

public sealed class CreateDeliveryManCommandValidator : AbstractValidator<CreateDeliveryManCommand>
{
    public CreateDeliveryManCommandValidator(IDeliveryManRepository repo)
    {
        RuleFor(x => x.Identificador).NotEmpty();
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(120);

        RuleFor(x => x.Cnpj)
            .Cascade(CascadeMode.Stop)
            .Must(c => OnlyDigits(c).Length == 14).WithMessage("CNPJ inválido (14 dígitos).")
            .MustAsync(async (c, ct) => !await repo.CnpjExistsAsync(OnlyDigits(c), ct))
            .WithMessage("CNPJ já cadastrado.");

        RuleFor(x => x.NumeroCnh)
            .Cascade(CascadeMode.Stop)
            .Must(c => OnlyDigits(c).Length == 11).WithMessage("CNH inválida (11 dígitos).")
            .MustAsync(async (c, ct) => !await repo.CnhExistsAsync(OnlyDigits(c), ct))
            .WithMessage("CNH já cadastrada.");

        RuleFor(x => x.TipoCnh)
            .NotEmpty()
            .Must(v => CnhEnumConverter.TryParse(v, out _))
            .WithMessage("Tipo de CNH inválido. Válidos: A, B, A+B.");

        When(x => !string.IsNullOrWhiteSpace(x.ImagemCnh), () =>
        {
            RuleFor(x => x.ImagemCnh!)
                .Must(IsValidPngOrBmpBase64)
                .WithMessage("Imagem inválida. Envie PNG ou BMP (base64).");
        });

    }

    static string OnlyDigits(string s) => new string((s ?? "").Where(char.IsDigit).ToArray());

    private static bool IsValidPngOrBmpBase64(string input)
    {
        try
        {
            byte[] bytes;
            if (input.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                var headerEnd = input.IndexOf(";base64,", StringComparison.OrdinalIgnoreCase);
                if (headerEnd < 0) return false;
                var mime = input[5..headerEnd];
                if (mime is not ("image/png" or "image/bmp")) return false;
                var b64 = input[(headerEnd + 8)..];
                bytes = Convert.FromBase64String(b64);
            }
            else
            {
                bytes = Convert.FromBase64String(input);
            }

            return IsPng(bytes) || IsBmp(bytes);
        }
        catch { return false; }

        static bool IsPng(byte[] b) =>
            b.Length >= 8 &&
            b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47 &&
            b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A;

        static bool IsBmp(byte[] b) =>
            b.Length >= 2 && b[0] == 0x42 && b[1] == 0x4D;
    }
}
