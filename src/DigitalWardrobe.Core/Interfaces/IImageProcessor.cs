namespace DigitalWardrobe.Core.Interfaces;

public interface IImageProcessor
{
    Task<Stream?> ToWebPAsync(Stream input);
}