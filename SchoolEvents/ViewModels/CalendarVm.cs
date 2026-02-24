using System;
using System.Collections.Generic;
using SchoolEvents.Models;

namespace SchoolEvents.ViewModels;

public class CalendarVm
{
    public int Year { get; set; }
    public int Month { get; set; }
    public Dictionary<DateTime, List<SchoolEvent>> EventsByDay { get; set; } = new();
}
