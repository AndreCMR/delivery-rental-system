namespace delivery_rental_system.Domain.Enums;

public static class CnhEnumConverter
{
    public static bool TryParse(string? value, out CnhEnum result)
    {
        var parsed = value?.Trim().ToUpperInvariant() switch
        {
            "A" => (CnhEnum?)CnhEnum.A,
            "B" => CnhEnum.B,
            "A+B" => CnhEnum.AB,
            "AB" => CnhEnum.AB,
            _ => null
        };

        if (parsed is { } ok)
        {
            result = ok;
            return true;
        }

        result = default; 
        return false;
    }


    public static string ToApiString(this CnhEnum value) => value switch
    {
        CnhEnum.A => "A",
        CnhEnum.B => "B",
        CnhEnum.AB => "A+B",
        _ => "A"
    };
}