using delivery_rental_system.Application.Abstractions;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace delivery_rental_system.Infra.Storage;


public sealed class MinioStorage : IFileStorage
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private readonly string _prefix;
    private readonly bool _usePresigned;
    private readonly int _presignedExpirySeconds;
    private readonly string? _publicBaseUrl;

    public MinioStorage(
        IMinioClient client,
        string bucket,
        string objectPrefix,
        bool usePresignedUrls,
        int presignedExpirySeconds,
        string? publicBaseUrl = null)
    {
        _client = client;
        _bucket = bucket;
        _prefix = string.IsNullOrWhiteSpace(objectPrefix) ? "" : objectPrefix.TrimStart('/');
        _usePresigned = usePresignedUrls;
        _presignedExpirySeconds = Math.Max(60, presignedExpirySeconds);
        _publicBaseUrl = publicBaseUrl?.TrimEnd('/');
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken ct)
    {
        await EnsureBucketAsync(ct);

        var ext = Path.GetExtension(fileName);
        var safeName = $"{Guid.NewGuid():N}{ext}".ToLowerInvariant();
        var objectName = string.IsNullOrEmpty(_prefix) ? safeName : $"{_prefix}{safeName}";

        Stream uploadStream = content;
        if (!content.CanSeek)
        {
            var ms = new MemoryStream();
            await content.CopyToAsync(ms, ct);
            ms.Position = 0;
            uploadStream = ms;
        }
        else
        {
            if (content.Position != 0) content.Position = 0;
        }

        var put = new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithStreamData(uploadStream)
            .WithObjectSize(uploadStream.Length)
            .WithContentType(contentType);

        await _client.PutObjectAsync(put, ct);

        if (_usePresigned)
        {
            var presign = new PresignedGetObjectArgs()
                .WithBucket(_bucket)
                .WithObject(objectName)
                .WithExpiry(_presignedExpirySeconds);
            return await _client.PresignedGetObjectAsync(presign);
        }

        if (string.IsNullOrWhiteSpace(_publicBaseUrl))
            throw new InvalidOperationException("PublicBaseUrl não configurado para bucket público.");

        return $"{_publicBaseUrl}/{_bucket}/{Uri.EscapeDataString(objectName)}";
    }

    public async Task DeleteIfExistsAsync(string urlOrObjectName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(urlOrObjectName))
            return;

        string objectName;

        if (urlOrObjectName.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(urlOrObjectName);

            var path = uri.AbsolutePath.TrimStart('/');

            if (path.StartsWith($"{_bucket}/", StringComparison.OrdinalIgnoreCase))
                path = path.Substring(_bucket.Length + 1);

            objectName = path;
        }
        else
        {
            objectName = urlOrObjectName;
        }

        if (string.IsNullOrWhiteSpace(objectName))
            return;

        try
        {
            var args = new RemoveObjectArgs()
                .WithBucket(_bucket)
                .WithObject(objectName);

            await _client.RemoveObjectAsync(args, ct);
        }
        catch (ObjectNotFoundException)
        {
        }
        catch (Exception ex) when (
            ex.Message.Contains("NoSuchKey", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("Not Found", StringComparison.OrdinalIgnoreCase))
        {
        }
    }

    private async Task EnsureBucketAsync(CancellationToken ct)
    {
        var exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket), ct);
        if (!exists)
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket), ct);
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }
}