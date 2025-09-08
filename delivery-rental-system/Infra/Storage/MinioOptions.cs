using System.ComponentModel.DataAnnotations;

namespace delivery_rental_system.Infra.Storage;

public sealed class MinioOptions
{
    [Required] public string Endpoint { get; init; } = null!;
    [Required] public string AccessKey { get; init; } = null!;
    [Required] public string SecretKey { get; init; } = null!;
    public bool WithSSL { get; init; }

    [Required] public string Bucket { get; init; } = null!;
    public string? ObjectPrefix { get; init; }

    public bool UsePresignedUrls { get; init; } = true;

    [Range(60, int.MaxValue)]
    public int PresignedExpirySeconds { get; init; } = 604800;

    public string? PublicBaseUrl { get; init; }
}
