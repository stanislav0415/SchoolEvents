using System.Collections.Generic;
using SchoolEvents.Models;

namespace SchoolEvents.ViewModels;

public class EventIndexVm
{
    public EventFilterVm Filter { get; set; } = new();
    public List<SchoolEvent> Events { get; set; } = new();
}
