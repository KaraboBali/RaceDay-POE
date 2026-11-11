using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models;

public class EventFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter the event name."), StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose the date and time of the event.")]
    public DateTime? Date { get; set; } = DateTime.Today.AddDays(30).AddHours(8);

    [Required(ErrorMessage = "Please enter the location."), StringLength(150)]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose an event type.")]
    public string EventType { get; set; } = "Run";

    [Range(0, 1000, ErrorMessage = "Distance must be between 0 and 1000 km.")]
    public decimal? DistanceKm { get; set; }

    [StringLength(500, ErrorMessage = "The description can be up to 500 characters.")]
    public string? Description { get; set; }

    public IFormFile? BannerFile { get; set; }
    public string? ExistingBannerUrl { get; set; }
}

public class EventCategoriesViewModel
{
    public EventItem Event { get; set; } = new();
    public List<CategoryItem> Categories { get; set; } = new();
}

public class EventEnrolmentsViewModel
{
    public EventItem Event { get; set; } = new();
    public List<EventEnrolmentItem> Enrolments { get; set; } = new();
}