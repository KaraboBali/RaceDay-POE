using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models;

public class EventListViewModel
{
    public List<EventItem> Events { get; set; } = new();
    public string? Type { get; set; }
    public string? Search { get; set; }
    public bool IncludePast { get; set; }
}

public class EventDetailsViewModel
{
    public EventItem Event { get; set; } = new();
    public List<CategoryItem> Categories { get; set; } = new();
    public bool IsParticipant { get; set; }
    public MyEnrolmentItem? MyEnrolment { get; set; }
}

public class ProfileViewModel
{
    [Required(ErrorMessage = "Please enter your name."), StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address."),
     EmailAddress(ErrorMessage = "That email address does not look right.")]
    public string Email { get; set; } = string.Empty;

    public string? PictureUrl { get; set; }
    public string Role { get; set; } = string.Empty;
}