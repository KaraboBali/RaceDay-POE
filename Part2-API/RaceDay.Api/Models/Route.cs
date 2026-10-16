namespace RaceDay.Api.Models;

public class Route
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string? Description { get; set; }
    public decimal? DistanceKm { get; set; }
    public string? MapUrl { get; set; }

    public Event Event { get; set; } = null!;
}
