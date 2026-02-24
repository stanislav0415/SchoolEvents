using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolEvents.Data;
using SchoolEvents.Models;

namespace SchoolEvents.Controllers;

[Authorize]
public class RegistrationsController : Controller
{
    private readonly ApplicationDbContext _db;
    public RegistrationsController(ApplicationDbContext db) => _db = db;

    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Index()
    {
        var regs = await _db.EventRegistrations
            .Include(r => r.SchoolEvent)
            .Include(r => r.Participant)
            .AsNoTracking()
            .OrderByDescending(r => r.RegisteredAt)
            .ToListAsync();

        return View(regs);
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Register(int eventId)
    {
        var ev = await _db.SchoolEvents.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
        if (ev is null) return NotFound();

        ViewBag.EventTitle = ev.Title;
        ViewBag.EventId = eventId;
        return View(new Participant());
    }

    [Authorize(Roles = "Student")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int eventId, Participant participant)
    {
        var ev = await _db.SchoolEvents
            .Include(e => e.Registrations)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (ev is null) return NotFound();

        int currentCount = await _db.EventRegistrations.CountAsync(r => r.SchoolEventId == eventId);
        if (currentCount >= ev.Capacity)
            ModelState.AddModelError("", "There is no available seats for this event.");

        if (!ModelState.IsValid)
        {
            ViewBag.EventTitle = ev.Title;
            ViewBag.EventId = eventId;
            return View(participant);
        }

        Participant? existing = null;
        if (!string.IsNullOrWhiteSpace(participant.Email))
            existing = await _db.Participants.FirstOrDefaultAsync(p => p.Email == participant.Email);

        var pEntity = existing ?? participant;
        if (existing is null)
            _db.Participants.Add(pEntity);

        await _db.SaveChangesAsync();

        bool already = await _db.EventRegistrations.AnyAsync(r =>
            r.SchoolEventId == eventId && r.ParticipantId == pEntity.Id);

        if (already)
        {
            TempData["Msg"] = "You`re already registrated for this event.";
            return RedirectToAction("Details", "Events", new { id = eventId });
        }

        _db.EventRegistrations.Add(new EventRegistration
        {
            SchoolEventId = eventId,
            ParticipantId = pEntity.Id,
            RegisteredAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        TempData["Msg"] = "Succesfull registration!";
        return RedirectToAction("Details", "Events", new { id = eventId });
    }
}
