namespace RaceDay.Api.Models;

public class Category
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }

    public Event Event { get; set; } = null!;
    public List<EventEnrolment> Enrolments { get; set; } = new();
}
