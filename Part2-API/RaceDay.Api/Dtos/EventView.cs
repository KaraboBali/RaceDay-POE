namespace RaceDay.Api.Dtos;

public class EventView
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string EventType { get; set; } = string.Empty;
    public decimal? DistanceKm { get; set; }
    public string? BannerUrl { get; set; }
    public int OrganiserId { get; set; }
    public int EnrolmentCount { get; set; }
}