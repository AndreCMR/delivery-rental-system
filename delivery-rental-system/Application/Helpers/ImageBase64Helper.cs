namespace delivery_rental_system.Application.Helpers;


public static class ImageBase64Helper
{
    /// <summary>
    /// Converte base64 (com ou sem data URL) em stream PNG/BMP, retornando (stream, fileName, contentType).
    /// Lança InvalidOperationException("Dados inválidos") para entradas inválidas.
    /// </summary>
    public static (Stream stream, string fileName, string contentType) ToStreamPngOrBmp(string input)
    {
        try
        {
            byte[] bytes;
            string mime, ext;

            if (input.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                var i = input.IndexOf(";base64,", StringComparison.OrdinalIgnoreCase);
                if (i <= 0) throw new InvalidOperationException("Dados inválidos");

                var header = input[..i];             
                mime = header[5..];               
                bytes = Convert.FromBase64String(input[(i + 8)..]);
                ext = mime.Equals("image/png", StringComparison.OrdinalIgnoreCase) ? ".png" : ".bmp";
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
        catch (FormatException)
        {
            throw new InvalidOperationException("Dados inválidos");
        }
    }
}