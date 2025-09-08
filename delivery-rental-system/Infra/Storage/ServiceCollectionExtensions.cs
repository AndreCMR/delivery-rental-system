using delivery_rental_system.Application.Abstractions;
using Microsoft.Extensions.Options;
using Minio;

namespace delivery_rental_system.Infra.Storage;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMinioStorage(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddOptions<MinioOptions>()
            .Bind(config.GetSection("Minio"))
            .ValidateDataAnnotations()
            .Validate(o => !(!o.UsePresignedUrls && string.IsNullOrWhiteSpace(o.PublicBaseUrl)),
                "PublicBaseUrl é obrigatório quando UsePresignedUrls = false")
            .ValidateOnStart();

        services.AddSingleton<IMinioClient>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<MinioOptions>>().Value;
            var client = new MinioClient()
                .WithEndpoint(opt.Endpoint)
                .WithCredentials(opt.AccessKey, opt.SecretKey);
            if (opt.WithSSL) client = client.WithSSL();
            return client.Build();
        });

        services.AddSingleton<IFileStorage>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<MinioOptions>>().Value;
            var cli = sp.GetRequiredService<IMinioClient>();
            return new MinioStorage(
                cli,
                opt.Bucket,
                opt.ObjectPrefix ?? string.Empty,
                opt.UsePresignedUrls,
                opt.PresignedExpirySeconds,
                opt.PublicBaseUrl
            );
        });

        return services;
    }
}