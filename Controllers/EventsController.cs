using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolEvents.Data;
using SchoolEvents.Models;
using SchoolEvents.ViewModels;

namespace SchoolEvents.Controllers;

public class EventsController : Controller
{
    private readonly ApplicationDbContext _db;
    public EventsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(EventFilterVm filter)
    {
        var q = _db.SchoolEvents
            .Include(e => e.Organizer)
            .AsQueryable();

        if (filter.From.HasValue)
            q = q.Where(e => e.StartAt.Date >= filter.From.Value.Date);

        if (filter.To.HasValue)
            q = q.Where(e => e.StartAt.Date <= filter.To.Value.Date);

        if (filter.Type.HasValue)
            q = q.Where(e => e.Type == filter.Type.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            q = q.Where(e => e.Title.Contains(filter.Search) || (e.Location ?? "").Contains(filter.Search));

        var vm = new EventIndexVm
        {
            Filter = filter,
            Events = await q.OrderBy(e => e.StartAt).AsNoTracking().ToListAsync()
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var ev = await _db.SchoolEvents
            .Include(e => e.Organizer)
            .Include(e => e.Registrations)
                .ThenInclude(r => r.Participant)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        return ev is null ? NotFound() : View(ev);
    }

    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Create()
    {
        await LoadOrganizersAsync();
        return View(new SchoolEvent
        {
            StartAt = DateTime.Now.AddDays(1),
            EndAt = DateTime.Now.AddDays(1).AddHours(2)
        });
    }

    [Authorize(Roles = "Teacher")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SchoolEvent ev)
    {
        if (ev.EndAt <= ev.StartAt)
            ModelState.AddModelError(nameof(ev.EndAt), "The end should be after the beggining.");

        if (!ModelState.IsValid)
        {
            await LoadOrganizersAsync(ev.OrganizerId);
            return View(ev);
        }

        _db.SchoolEvents.Add(ev);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var ev = await _db.SchoolEvents.FindAsync(id);
        if (ev is null) return NotFound();
        await LoadOrganizersAsync(ev.OrganizerId);
        return View(ev);
    }

    [Authorize(Roles = "Teacher")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SchoolEvent ev)
    {
        if (id != ev.Id) return NotFound();

        if (ev.EndAt <= ev.StartAt)
            ModelState.AddModelError(nameof(ev.EndAt), "The end should be after the beggining.");

        if (!ModelState.IsValid)
        {
            await LoadOrganizersAsync(ev.OrganizerId);
            return View(ev);
        }

        try
        {
            _db.Update(ev);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.SchoolEvents.AnyAsync(e => e.Id == ev.Id)) return NotFound();
            throw;
        }
    }

    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var ev = await _db.SchoolEvents
            .Include(e => e.Organizer)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
        return ev is null ? NotFound() : View(ev);
    }

    [Authorize(Roles = "Teacher")]
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ev = await _db.SchoolEvents.FindAsync(id);
        if (ev is null) return NotFound();

        bool hasRegs = await _db.EventRegistrations.AnyAsync(r => r.SchoolEventId == id);
        if (hasRegs)
        {
            ModelState.AddModelError("", "You can`t delete an event with registration");
            return View("Delete", ev);
        }

        _db.SchoolEvents.Remove(ev);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadOrganizersAsync(int? selectedId = null)
    {
        var organizers = await _db.Organizers
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .ToListAsync();

        ViewBag.OrganizerId = new SelectList(organizers, "Id", "Name", selectedId);
    }
}
