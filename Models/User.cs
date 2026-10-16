namespace RaceDay.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Participant";   // "Organiser" or "Participant"

    public List<Event> OrganisedEvents { get; set; } = new();
    public List<EventEnrolment> Enrolments { get; set; } = new();
}
