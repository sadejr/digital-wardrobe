namespace DigitalWardrobe.Core.Interfaces;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string extension);

    Task<Stream?> OpenReadAsync(string key);

    Task DeleteAsync(string key);
}