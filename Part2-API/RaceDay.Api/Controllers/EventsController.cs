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

    [HttpGet]
    [SessionAuthorize]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Events.Select(e => new
        {
            e.Id,
            e.Name,
            e.Date,
            e.Location,
            e.Description,
            EventType = e.EventType.ToString(),
            e.OrganiserId
        }).ToListAsync());

    [HttpGet("{id}")]
    [SessionAuthorize]
    public async Task<IActionResult> Get(int id)
    {
        var e = await _db.Events.FindAsync(id);
        if (e == null) return NotFound(new { message = "Event not found." });
        return Ok(new
        {
            e.Id,
            e.Name,
            e.Date,
            e.Location,
            e.Description,
            EventType = e.EventType.ToString(),
            e.OrganiserId
        });
    }

    [HttpPost]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Create(EventDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Location))
            return BadRequest(new { message = "Name and location are required." });

        var e = new Event
        {
            OrganiserId = HttpContext.Session.GetInt32("UserId")!.Value,
            Name = dto.Name,
            Date = dto.Date,
            Location = dto.Location,
            Description = dto.Description,
            EventType = ParseType(dto.EventType)
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
            EventType = e.EventType.ToString()
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

        e.Name = dto.Name;
        e.Date = dto.Date;
        e.Location = dto.Location;
        e.Description = dto.Description;
        e.EventType = ParseType(dto.EventType);
        await _db.SaveChangesAsync();
        return Ok(new
        {
            e.Id,
            e.Name,
            e.Date,
            e.Location,
            e.Description,
            EventType = e.EventType.ToString()
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

        _db.Events.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}