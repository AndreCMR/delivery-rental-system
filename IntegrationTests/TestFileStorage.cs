
using delivery_rental_system.Application.Abstractions;
using System.Collections.Concurrent;

namespace IntegrationTests;

public sealed class TestFileStorage : IFileStorage
{
    public record Saved(string FileName, string ContentType, byte[] Bytes);

    private readonly ConcurrentDictionary<string, Saved> _files =
     new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<Saved> SavedFiles => _files.Values;

    public Task<string> SaveAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);

        _files[fileName] = new Saved(fileName, contentType, ms.ToArray());

        return Task.FromResult($"https://test-storage/{fileName}");
    }

    public Task DeleteIfExistsAsync(string fileUrlOrName, CancellationToken ct = default)
    {
        var name = NormalizeName(fileUrlOrName);
        var removed = _files.TryRemove(name, out _);
        return Task.FromResult(removed);
    }

    public void Clear() => _files.Clear();


    private static string NormalizeName(string fileUrlOrName)
    {
        if (string.IsNullOrWhiteSpace(fileUrlOrName))
            return string.Empty;

        if (Uri.TryCreate(fileUrlOrName, UriKind.Absolute, out var uri))
            return Path.GetFileName(uri.LocalPath);

        return fileUrlOrName;
    }
}