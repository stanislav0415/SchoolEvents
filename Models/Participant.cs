using System.ComponentModel.DataAnnotations;

namespace SchoolEvents.Models;

public class Participant
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? ClassName { get; set; }

    public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
}
