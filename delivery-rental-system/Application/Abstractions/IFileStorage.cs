namespace delivery_rental_system.Application.Abstractions;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken ct);

    Task DeleteIfExistsAsync(string urlOrObjectName, CancellationToken ct);

    void Clear();
}

