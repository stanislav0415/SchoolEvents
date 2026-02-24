using System.ComponentModel.DataAnnotations;

namespace SchoolEvents.Models;

public class EventRegistration
{
    public int Id { get; set; }

    [Required]
    public int SchoolEventId { get; set; }
    public SchoolEvent? SchoolEvent { get; set; }

    [Required]
    public int ParticipantId { get; set; }
    public Participant? Participant { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}
