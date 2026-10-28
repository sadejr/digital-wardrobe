using DigitalWardrobe.Core.Enums;

namespace DigitalWardrobe.Core.Models;

public class ClothingItemInput
{
    public string Name { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public Category? Category { get; set; }

    public List<string> Colours { get; set; } = new();

    public List<Season> Seasons { get; set; } = new();
}