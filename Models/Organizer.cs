using System.ComponentModel.DataAnnotations;

namespace SchoolEvents.Models;

public class Organizer
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Email { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(200)]
    public string? Department { get; set; }

    public ICollection<SchoolEvent> Events { get; set; } = new List<SchoolEvent>();
}
