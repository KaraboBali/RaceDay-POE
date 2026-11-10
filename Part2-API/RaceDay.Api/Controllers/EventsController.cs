using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[RouteAttribute("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayContext _db;
    public EventsController(RaceDayContext db) => _db = db;

    private static EventType ParseType(string? t) =>
        Enum.TryParse<EventType>(t, true, out var r) ? r : EventType.Run;

    private static IQueryable<EventView> Project(IQueryable<Event> q) =>
        q.Select(e => new EventView
        {
            Id = e.Id,
            Name = e.Name,
            Date = e.Date,
            Location = e.Location,
            Description = e.Description,
            EventType = e.EventType.ToString(),
            DistanceKm = e.DistanceKm,
            BannerUrl = e.BannerUrl,
            OrganiserId = e.OrganiserId,
            EnrolmentCount = e.Categories.SelectMany(c => c.Enrolments).Count()
        });

    // Optional filters: type, search, upcomingOnly, organiserId
    [HttpGet]
    [SessionAuthorize]
    public async Task<IActionResult> GetAll(
        string? type = null, string? search = null, bool upcomingOnly = false, int? organiserId = null)
    {
        var query = _db.Events.AsQueryable();

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<EventType>(type, true, out var parsed))
            query = query.Where(e => e.EventType == parsed);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.Name.Contains(search) || e.Location.Contains(search));
        if (upcomingOnly)
        {
            var today = DateTime.Today;
            query = query.Where(e => e.Date >= today);
        }
        if (organiserId.HasValue)
            query = query.Where(e => e.OrganiserId == organiserId.Value);

        return Ok(await Project(query).OrderBy(e => e.Date).ToListAsync());
    }

    [HttpGet("{id}")]
    [SessionAuthorize]
    public async Task<IActionResult> Get(int id)
    {
        var e = await Project(_db.Events.Where(x => x.Id == id)).FirstOrDefaultAsync();
        if (e == null) return NotFound(new { message = "Event not found." });
        return Ok(e);
    }

    [HttpPost]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Create(EventDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Location))
            return BadRequest(new { message = "Name and location are required." });
        if (dto.DistanceKm is < 0)
            return BadRequest(new { message = "Distance cannot be negative." });

        var e = new Event
        {
            OrganiserId = HttpContext.Session.GetInt32("UserId")!.Value,
            Name = dto.Name,
            Date = dto.Date,
            Location = dto.Location,
            Description = dto.Description,
            EventType = ParseType(dto.EventType),
            DistanceKm = dto.DistanceKm
        };
        _db.Events.Add(e);
        await _db.SaveChangesAsync();
        return StatusCode(201, new
        {
            e.Id,
            e.Name,
            e.Date,
            e.Location,
            e.Description,
            EventType = e.EventType.ToString(),
            e.DistanceKm,
            e.BannerUrl
        });
    }

    [HttpPut("{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Update(int id, EventDto dto)
    {
        var e = await _db.Events.FindAsync(id);
        if (e == null) return NotFound(new { message = "Event not found." });
        if (e.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            return StatusCode(403, new { message = "You can only edit your own events." });
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Location))
            return BadRequest(new { message = "Name and location are required." });
        if (dto.DistanceKm is < 0)
            return BadRequest(new { message = "Distance cannot be negative." });

        e.Name = dto.Name;
        e.Date = dto.Date;
        e.Location = dto.Location;
        e.Description = dto.Description;
        e.EventType = ParseType(dto.EventType);
        e.DistanceKm = dto.DistanceKm;
        await _db.SaveChangesAsync();
        return Ok(new
        {
            e.Id,
            e.Name,
            e.Date,
            e.Location,
            e.Description,
            EventType = e.EventType.ToString(),
            e.DistanceKm,
            e.BannerUrl
        });
    }

    [HttpDelete("{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.Events.FindAsync(id);
        if (e == null) return NotFound(new { message = "Event not found." });
        if (e.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            return StatusCode(403, new { message = "You can only delete your own events." });

        var hasEnrolments = await _db.EventEnrolments.AnyAsync(x => x.Category.EventId == id);
        if (hasEnrolments)
            return Conflict(new { message = "This event already has enrolments and cannot be deleted." });

        _db.Events.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}