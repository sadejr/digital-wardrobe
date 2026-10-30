using DigitalWardrobe.Core.Interfaces;

namespace DigitalWardrobe.Data.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _rootPath;

    public LocalFileStorage(string rootPath)
    {
        _rootPath = rootPath;
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Stream content, string extension)
    {
        var key = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(_rootPath, key);

        await using var file = File.Create(path);
        await content.CopyToAsync(file);

        return key;
    }

    public Task<Stream?> OpenReadAsync(string key)
    {
        var path = GetSafePath(key);
        if (path == null || !File.Exists(path))
            return Task.FromResult<Stream?>(null);

        Stream stream = File.OpenRead(path);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string key)
    {
        var path = GetSafePath(key);
        if (path != null && File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    private string? GetSafePath(string key)
    {
        if (key != Path.GetFileName(key))
            return null;

        return Path.Combine(_rootPath, key);
    }
}