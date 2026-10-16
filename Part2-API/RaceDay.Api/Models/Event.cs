namespace RaceDay.Api.Models;

public enum EventType { Run, Walk, Cycle }

public class Event
{
    public int Id { get; set; }
    public int OrganiserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EventType EventType { get; set; }   // added to meet the Part 2 brief

    public User Organiser { get; set; } = null!;
    public Route? Route { get; set; }
    public List<Category> Categories { get; set; } = new();
}
