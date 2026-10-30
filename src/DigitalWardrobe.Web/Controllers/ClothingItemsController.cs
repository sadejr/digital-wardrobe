using System.Security.Claims;
using DigitalWardrobe.Core.Services;
using DigitalWardrobe.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DigitalWardrobe.Web.Controllers;

public class ClothingItemsController : Controller
{
    private readonly ClothingItemService _service;

    public ClothingItemsController(ClothingItemService service)
    {
        _service = service;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {
        var items = await _service.GetForUserAsync(CurrentUserId);
        return View(items);
    }

    public IActionResult Create()
    {
        return View(new ClothingItemFormViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(ClothingItemFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _service.CreateAsync(CurrentUserId, model.ToInput());
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }
}