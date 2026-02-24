using System;
using SchoolEvents.Models;

namespace SchoolEvents.ViewModels;

public class EventFilterVm
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public EventType? Type { get; set; }
    public string? Search { get; set; }
}
