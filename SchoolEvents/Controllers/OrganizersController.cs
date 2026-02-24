using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolEvents.Data;
using SchoolEvents.Models;

namespace SchoolEvents.Controllers;

[Authorize(Roles = "Teacher")]
public class OrganizersController(
    ApplicationDbContext db,
    UserManager<IdentityUser> userManager) : Controller
{
    private readonly ApplicationDbContext _db = db;
    private readonly UserManager<IdentityUser> _userManager = userManager;

    public async Task<IActionResult> Index()
        => View(await _db.Organizers.AsNoTracking().ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var org = await _db.Organizers.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        return org is null ? NotFound() : View(org);
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Organizer organizer)
    {
        var user = await _userManager.FindByEmailAsync(organizer.Email);
        if (user is null)
        {
            ModelState.AddModelError(nameof(organizer.Email),
                "No user exists with this email.");
        }

        if (!ModelState.IsValid) 
            return View(organizer);

        _db.Organizers.Add(organizer);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var org = await _db.Organizers.FindAsync(id);
        return org is null ? NotFound() : View(org);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Organizer organizer)
    {
        if (id != organizer.Id) return NotFound();
        if (!ModelState.IsValid) return View(organizer);

        try
        {
            _db.Update(organizer);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Organizers.AnyAsync(o => o.Id == organizer.Id)) return NotFound();
            throw;
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var org = await _db.Organizers.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        return org is null ? NotFound() : View(org);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var org = await _db.Organizers.FindAsync(id);
        if (org is null) return NotFound();

        bool hasEvents = await _db.SchoolEvents.AnyAsync(e => e.OrganizerId == id);
        if (hasEvents)
        {
            ModelState.AddModelError("", "You can`t delete an organizator, which have running events.");
            return View("Delete", org);
        }

        _db.Organizers.Remove(org);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
