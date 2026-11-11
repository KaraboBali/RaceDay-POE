namespace RaceDay.MVC.Models;

// These classes match the JSON that the Part 2 API sends back.
public class LoginInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class UserInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
}

public class EventItem
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

public class CategoryItem
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
}

// A participant's own enrolment
public class MyEnrolmentItem
{
    public int Id { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int EventId { get; set; }
    public string Event { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string EventLocation { get; set; } = string.Empty;
}

// An enrolment as the organiser sees it
public class EventEnrolmentItem
{
    public int Id { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ParticipantId { get; set; }
    public string Participant { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int? ResultId { get; set; }
    public int? Position { get; set; }
    public TimeSpan? FinishTime { get; set; }
}

public class MyResultItem
{
    public int Id { get; set; }
    public int Position { get; set; }
    public TimeSpan FinishTime { get; set; }
    public string Event { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Category { get; set; } = string.Empty;
    public int TotalFinishers { get; set; }
}