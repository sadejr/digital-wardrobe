using System.ComponentModel.DataAnnotations;
using DigitalWardrobe.Core.Enums;
using DigitalWardrobe.Core.Models;
using DigitalWardrobe.Core.Entities;

namespace DigitalWardrobe.Web.ViewModels;

public class ClothingItemFormViewModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Brand { get; set; }

    public Category? Category { get; set; }

    [Display(Name = "Colours")]
    [StringLength(200)]
    public string? ColoursText { get; set; }

    public List<Season> Seasons { get; set; } = new();

    public ClothingItemInput ToInput() => new()
    {
        Name = Name,
        Brand = Brand,
        Category = Category,
        Colours = (ColoursText ?? string.Empty).Split(',').ToList(),
        Seasons = Seasons
    };

    public static ClothingItemFormViewModel FromEntity(ClothingItem item) => new()
    {
        Name = item.Name,
        Brand = item.Brand,
        Category = item.Category,
        ColoursText = string.Join(", ", item.Colours),
        Seasons = item.Seasons.ToList()
    };
}