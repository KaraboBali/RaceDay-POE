using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayContext : DbContext
{
    public RaceDayContext(DbContextOptions<RaceDayContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EventEnrolment> EventEnrolments => Set<EventEnrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.ToTable("Users", t => t.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Organiser','Participant')"));
            e.Property(u => u.Name).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(150).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(u => u.Role).HasMaxLength(20).IsRequired().HasDefaultValue("Participant");
        });

        b.Entity<Event>(e =>
        {
            e.ToTable("Events");
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Location).HasMaxLength(150).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.EventType).HasConversion<string>().HasMaxLength(10);
            e.HasOne(x => x.Organiser).WithMany(u => u.OrganisedEvents)
                .HasForeignKey(x => x.OrganiserId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Route>(e =>
        {
            e.ToTable("Routes");
            e.Property(r => r.Description).HasMaxLength(300);
            e.Property(r => r.DistanceKm).HasPrecision(5, 2);
            e.Property(r => r.MapUrl).HasMaxLength(255);
            e.HasIndex(r => r.EventId).IsUnique();
            e.HasOne(r => r.Event).WithOne(x => x.Route).HasForeignKey<Route>(r => r.EventId);
        });

        b.Entity<Category>(e =>
        {
            e.ToTable("Categories");
            e.Property(c => c.Name).HasMaxLength(50).IsRequired();
            e.Property(c => c.DistanceKm).HasPrecision(5, 2);
            e.HasOne(c => c.Event).WithMany(x => x.Categories).HasForeignKey(c => c.EventId);
        });

        b.Entity<EventEnrolment>(e =>
        {
            e.ToTable("EventEnrolments");
            e.HasIndex(x => new { x.ParticipantId, x.CategoryId }).IsUnique();
            e.HasOne(x => x.Participant).WithMany(u => u.Enrolments)
                .HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category).WithMany(c => c.Enrolments)
                .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Result>(e =>
        {
            e.ToTable("Results");
            e.HasIndex(r => r.EnrolmentId).IsUnique();
            e.HasOne(r => r.Enrolment).WithOne(x => x.Result).HasForeignKey<Result>(r => r.EnrolmentId);
        });
    }
}