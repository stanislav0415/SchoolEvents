using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolEvents.Data;
using SchoolEvents.ViewModels;

namespace SchoolEvents.Controllers;

public class CalendarController : Controller
{
    private readonly ApplicationDbContext _db;
    public CalendarController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(int? year, int? month)
    {
        var now = DateTime.Now;
        int y = year ?? now.Year;
        int m = month ?? now.Month;

        var start = new DateTime(y, m, 1);
        var end = start.AddMonths(1);

        var events = await _db.SchoolEvents
            .AsNoTracking()
            .Where(e => e.StartAt >= start && e.StartAt < end)
            .OrderBy(e => e.StartAt)
            .ToListAsync();

        var vm = new CalendarVm
        {
            Year = y,
            Month = m,
            EventsByDay = events
                .GroupBy(e => e.StartAt.Date)
                .ToDictionary(g => g.Key, g => g.ToList())
        };

        return View(vm);
    }
}
