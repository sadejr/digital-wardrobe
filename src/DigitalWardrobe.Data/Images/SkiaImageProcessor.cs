using DigitalWardrobe.Core.Interfaces;
using SkiaSharp;

namespace DigitalWardrobe.Data.Images;

public class SkiaImageProcessor : IImageProcessor
{
    private const int MaxWidth = 1000;
    private const int MaxHeight = 1500;
    private const long MaxPixels = 50_000_000;
    private const int Quality = 80;

    private static readonly SKEncodedImageFormat[] AllowedFormats =
    {
        SKEncodedImageFormat.Jpeg,
        SKEncodedImageFormat.Png,
        SKEncodedImageFormat.Webp
    };

    public Task<Stream?> ToWebPAsync(Stream input)
    {
        using var codec = SKCodec.Create(input);
        if (codec == null || !AllowedFormats.Contains(codec.EncodedFormat))
            return Task.FromResult<Stream?>(null);

        if ((long)codec.Info.Width * codec.Info.Height > MaxPixels)
            return Task.FromResult<Stream?>(null);

        using var original = SKBitmap.Decode(codec);
        if (original == null)
            return Task.FromResult<Stream?>(null);

        using var rotated = Rotate(original, codec.EncodedOrigin);
        var upright = rotated ?? original;

        using var resized = Resize(upright);
        var final = resized ?? upright;

        using var image = SKImage.FromBitmap(final);
        using var data = image.Encode(SKEncodedImageFormat.Webp, Quality);

        Stream output = new MemoryStream(data.ToArray());
        return Task.FromResult<Stream?>(output);
    }

    private static SKBitmap? Rotate(SKBitmap bitmap, SKEncodedOrigin origin)
    {
        var degrees = origin switch
        {
            SKEncodedOrigin.RightTop => 90,
            SKEncodedOrigin.BottomRight => 180,
            SKEncodedOrigin.LeftBottom => 270,
            _ => 0
        };

        if (degrees == 0)
            return null;

        var swapSides = degrees != 180;
        var width = swapSides ? bitmap.Height : bitmap.Width;
        var height = swapSides ? bitmap.Width : bitmap.Height;

        var result = new SKBitmap(width, height);
        using var canvas = new SKCanvas(result);
        canvas.Translate(width / 2f, height / 2f);
        canvas.RotateDegrees(degrees);
        canvas.Translate(-bitmap.Width / 2f, -bitmap.Height / 2f);
        canvas.DrawBitmap(bitmap, 0, 0);

        return result;
    }

    private static SKBitmap? Resize(SKBitmap bitmap)
    {
        var scale = Math.Min(
            (double)MaxWidth / bitmap.Width,
            (double)MaxHeight / bitmap.Height);

        if (scale >= 1)
            return null;

        var width = (int)Math.Round(bitmap.Width * scale);
        var height = (int)Math.Round(bitmap.Height * scale);

        return bitmap.Resize(
            new SKImageInfo(width, height),
            new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
    }
}