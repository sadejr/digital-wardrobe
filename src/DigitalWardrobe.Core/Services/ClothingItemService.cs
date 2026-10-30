using DigitalWardrobe.Core.Entities;
using DigitalWardrobe.Core.Interfaces;
using DigitalWardrobe.Core.Models;

namespace DigitalWardrobe.Core.Services;

public class ClothingItemService
{
    private readonly IClothingItemRepository _items;
    private readonly IFileStorage _storage;
    private readonly IImageProcessor _images;
    private readonly WardrobeLimits _limits;

    public ClothingItemService(
        IClothingItemRepository items,
        IFileStorage storage,
        IImageProcessor images,
        WardrobeLimits limits)
    {
        _items = items;
        _storage = storage;
        _images = images;
        _limits = limits;
    }

    public Task<List<ClothingItem>> GetForUserAsync(string userId)
        => _items.GetForUserAsync(userId);

    public Task<ClothingItem?> GetAsync(int id, string userId)
        => _items.GetByIdAsync(id, userId);

    public async Task<ServiceResult> CreateAsync(string userId, ClothingItemInput input)
    {
        var error = Validate(input);
        if (error != null)
            return ServiceResult.Invalid(error);

        var count = await _items.CountForUserAsync(userId);
        if (count >= _limits.MaxItemsPerUser)
            return ServiceResult.Invalid($"You can have at most {_limits.MaxItemsPerUser} items.");

        string? newKey = null;
        if (input.Photo != null)
        {
            var photo = await SavePhotoAsync(input.Photo);
            if (photo.Error != null)
                return ServiceResult.Invalid(photo.Error);

            newKey = photo.Key;
        }

        var now = DateTime.UtcNow;
        var item = new ClothingItem
        {
            UserId = userId,
            ImageKey = newKey,
            CreatedAt = now,
            UpdatedAt = now
        };
        Apply(input, item);

        try
        {
            await _items.AddAsync(item);
        }
        catch
        {
            if (newKey != null)
                await _storage.DeleteAsync(newKey);

            throw;
        }

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateAsync(int id, string userId, ClothingItemInput input)
    {
        var item = await _items.GetByIdAsync(id, userId);
        if (item == null)
            return ServiceResult.NotFound();

        var error = Validate(input);
        if (error != null)
            return ServiceResult.Invalid(error);

        var oldKey = item.ImageKey;
        string? newKey = null;

        if (input.Photo != null)
        {
            var photo = await SavePhotoAsync(input.Photo);
            if (photo.Error != null)
                return ServiceResult.Invalid(photo.Error);

            newKey = photo.Key;
            item.ImageKey = newKey;
        }
        else if (input.RemovePhoto)
        {
            item.ImageKey = null;
        }

        Apply(input, item);
        item.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _items.UpdateAsync(item);
        }
        catch
        {
            if (newKey != null)
                await _storage.DeleteAsync(newKey);

            throw;
        }

        if (oldKey != null && oldKey != item.ImageKey)
            await _storage.DeleteAsync(oldKey);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id, string userId)
    {
        var item = await _items.GetByIdAsync(id, userId);
        if (item == null)
            return ServiceResult.NotFound();

        await _items.DeleteAsync(item);

        if (item.ImageKey != null)
            await _storage.DeleteAsync(item.ImageKey);

        return ServiceResult.Ok();
    }

    public async Task<Stream?> OpenPhotoAsync(int id, string userId)
    {
        var item = await _items.GetByIdAsync(id, userId);
        if (item?.ImageKey == null)
            return null;

        return await _storage.OpenReadAsync(item.ImageKey);
    }

    private async Task<(string? Key, string? Error)> SavePhotoAsync(Stream photo)
    {
        await using var webp = await _images.ToWebPAsync(photo);
        if (webp == null)
            return (null, "The photo must be a JPEG, PNG or WebP image.");

        var key = await _storage.SaveAsync(webp, ".webp");
        return (key, null);
    }

    private string? Validate(ClothingItemInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            return "Name is required.";

        if (input.Category.HasValue && !Enum.IsDefined(input.Category.Value))
            return "Unknown category.";

        if (input.Seasons.Any(s => !Enum.IsDefined(s)))
            return "Unknown season.";

        var maxBytes = _limits.MaxPhotoSizeMb * 1024L * 1024L;
        if (input.Photo != null && input.PhotoSize > maxBytes)
            return $"The photo must be at most {_limits.MaxPhotoSizeMb} MB.";

        return null;
    }

    private static void Apply(ClothingItemInput input, ClothingItem item)
    {
        item.Name = input.Name.Trim();
        item.Brand = string.IsNullOrWhiteSpace(input.Brand) ? null : input.Brand.Trim();
        item.Category = input.Category;
        item.Colours = input.Colours
            .Select(c => c.Trim().ToLowerInvariant())
            .Where(c => c.Length > 0)
            .Distinct()
            .ToList();
        item.Seasons = input.Seasons.Distinct().ToList();
    }
}