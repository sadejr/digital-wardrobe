using DigitalWardrobe.Core.Entities;

namespace DigitalWardrobe.Core.Interfaces;

public interface IClothingItemRepository
{
    Task<ClothingItem?> GetByIdAsync(int id, string userId);

    Task<List<ClothingItem>> GetForUserAsync(string userId);

    Task<int> CountForUserAsync(string userId);

    Task AddAsync(ClothingItem item);

    Task UpdateAsync(ClothingItem item);

    Task DeleteAsync(ClothingItem item);
}