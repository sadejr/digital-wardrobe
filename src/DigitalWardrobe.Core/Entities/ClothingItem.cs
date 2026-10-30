using DigitalWardrobe.Core.Enums;

namespace DigitalWardrobe.Core.Entities;

public class ClothingItem
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public Category? Category { get; set; }

    public List<string> Colours { get; set; } = new();

    public List<Season> Seasons { get; set; } = new();

    public string? ImageKey { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}