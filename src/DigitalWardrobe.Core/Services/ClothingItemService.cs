using DigitalWardrobe.Core.Entities;
using DigitalWardrobe.Core.Interfaces;
using DigitalWardrobe.Core.Models;

namespace DigitalWardrobe.Core.Services;

public class ClothingItemService
{
    private readonly IClothingItemRepository _items;
    private readonly WardrobeLimits _limits;

    public ClothingItemService(IClothingItemRepository items, WardrobeLimits limits)
    {
        _items = items;
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

        var now = DateTime.UtcNow;
        var item = new ClothingItem
        {
            UserId = userId,
            CreatedAt = now,
            UpdatedAt = now
        };
        Apply(input, item);

        await _items.AddAsync(item);
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

        Apply(input, item);
        item.UpdatedAt = DateTime.UtcNow;

        await _items.UpdateAsync(item);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id, string userId)
    {
        var item = await _items.GetByIdAsync(id, userId);
        if (item == null)
            return ServiceResult.NotFound();

        await _items.DeleteAsync(item);
        return ServiceResult.Ok();
    }

    private static string? Validate(ClothingItemInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            return "Name is required.";

        if (input.Category.HasValue && !Enum.IsDefined(input.Category.Value))
            return "Unknown category.";

        if (input.Seasons.Any(s => !Enum.IsDefined(s)))
            return "Unknown season.";

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