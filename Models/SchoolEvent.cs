using System.ComponentModel.DataAnnotations;

namespace SchoolEvents.Models;

public class SchoolEvent
{
    public int Id { get; set; }

    [Required, StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [StringLength(600)]
    public string? Description { get; set; }

    [Required]
    public DateTime StartAt { get; set; }

    [Required]
    public DateTime EndAt { get; set; }

    [Required]
    public EventType Type { get; set; } = EventType.Other;

    [StringLength(160)]
    public string? Location { get; set; }

    [Range(1, 500)]
    public int Capacity { get; set; } = 30;

    [Required]
    public int OrganizerId { get; set; }

    public Organizer? Organizer { get; set; }

    public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
}
