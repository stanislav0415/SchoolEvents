using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolEvents.Models;

namespace SchoolEvents.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Organizer> Organizers => Set<Organizer>();
    public DbSet<SchoolEvent> SchoolEvents => Set<SchoolEvent>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
}
