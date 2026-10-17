using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayContext _db;
    public CategoriesController(RaceDayContext db) => _db = db;

    [HttpGet("api/events/{eventId}/categories")]
    [SessionAuthorize]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.Id == eventId))
            return NotFound(new { message = "Event not found." });

        return Ok(await _db.Categories.Where(c => c.EventId == eventId)
            .Select(c => new { c.Id, c.EventId, c.Name, c.DistanceKm }).ToListAsync());
    }

    [HttpPost("api/events/{eventId}/categories")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Create(int eventId, CategoryDto dto)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            return StatusCode(403, new { message = "You can only add categories to your own events." });
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.DistanceKm <= 0)
            return BadRequest(new { message = "Name and a positive distance are required." });

        var c = new Category { EventId = eventId, Name = dto.Name, DistanceKm = dto.DistanceKm };
        _db.Categories.Add(c);
        await _db.SaveChangesAsync();
        return StatusCode(201, new { c.Id, c.EventId, c.Name, c.DistanceKm });
    }

    [HttpPut("api/categories/{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Update(int id, CategoryDto dto)
    {
        var c = await _db.Categories.Include(x => x.Event).FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound(new { message = "Category not found." });
        if (c.Event.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            return StatusCode(403, new { message = "You can only edit your own categories." });

        c.Name = dto.Name;
        c.DistanceKm = dto.DistanceKm;
        await _db.SaveChangesAsync();
        return Ok(new { c.Id, c.EventId, c.Name, c.DistanceKm });
    }

    [HttpDelete("api/categories/{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Categories.Include(x => x.Event).FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound(new { message = "Category not found." });
        if (c.Event.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            return StatusCode(403, new { message = "You can only delete your own categories." });

        _db.Categories.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}