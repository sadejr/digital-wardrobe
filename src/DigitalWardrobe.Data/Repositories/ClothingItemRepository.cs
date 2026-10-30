using DigitalWardrobe.Core.Entities;
using DigitalWardrobe.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalWardrobe.Data.Repositories;

public class ClothingItemRepository : IClothingItemRepository
{
    private readonly AppDbContext _db;

    public ClothingItemRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<ClothingItem?> GetByIdAsync(int id, string userId)
    {
        return _db.ClothingItems
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
    }

    public Task<List<ClothingItem>> GetForUserAsync(string userId)
    {
        return _db.ClothingItems
            .AsNoTracking()
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public Task<int> CountForUserAsync(string userId)
    {
        return _db.ClothingItems.CountAsync(i => i.UserId == userId);
    }

    public async Task AddAsync(ClothingItem item)
    {
        _db.ClothingItems.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ClothingItem item)
    {
        _db.ClothingItems.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ClothingItem item)
    {
        _db.ClothingItems.Remove(item);
        await _db.SaveChangesAsync();
    }
}