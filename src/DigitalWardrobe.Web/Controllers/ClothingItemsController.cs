using System.Security.Claims;
using DigitalWardrobe.Core.Services;
using DigitalWardrobe.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using DigitalWardrobe.Core.Models;

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

    public async Task<IActionResult> Details(int id)
    {
        var item = await _service.GetAsync(id, CurrentUserId);
        if (item == null)
            return NotFound();

        return View(item);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _service.GetAsync(id, CurrentUserId);
        if (item == null)
            return NotFound();

        return View(ClothingItemFormViewModel.FromEntity(item));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, ClothingItemFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _service.UpdateAsync(id, CurrentUserId, model.ToInput());
        if (result.Status == ResultStatus.NotFound)
            return NotFound();

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var item = await _service.GetAsync(id, CurrentUserId);
        if (item == null)
            return NotFound();

        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _service.DeleteAsync(id, CurrentUserId);
        if (result.Status == ResultStatus.NotFound)
            return NotFound();

        return RedirectToAction(nameof(Index));
    }  
}