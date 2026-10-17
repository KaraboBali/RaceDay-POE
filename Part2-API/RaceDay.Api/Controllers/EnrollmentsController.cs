using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayContext _db;
    public EnrolmentsController(RaceDayContext db) => _db = db;

    // Participant enrols in a category
    [HttpPost("api/categories/{categoryId}/enrolments")]
    [SessionAuthorize("Participant")]
    public async Task<IActionResult> Enrol(int categoryId)
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;

        var category = await _db.Categories.FindAsync(categoryId);
        if (category == null) return NotFound(new { message = "Category not found." });

        // One enrolment per event per participant
        var alreadyInEvent = await _db.EventEnrolments
            .AnyAsync(x => x.ParticipantId == userId && x.Category.EventId == category.EventId);
        if (alreadyInEvent)
            return Conflict(new { message = "You are already enrolled in this event." });

        var enrolment = new EventEnrolment { ParticipantId = userId, CategoryId = categoryId };
        _db.EventEnrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        return StatusCode(201, new { enrolment.Id, enrolment.CategoryId, enrolment.EnrolmentDate });
    }

    // Participant views own enrolments
    [HttpGet("api/enrolments/mine")]
    [SessionAuthorize("Participant")]
    public async Task<IActionResult> Mine()
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var list = await _db.EventEnrolments
            .Where(x => x.ParticipantId == userId)
            .Select(x => new
            {
                x.Id,
                x.EnrolmentDate,
                Category = x.Category.Name,
                Event = x.Category.Event.Name,
                EventDate = x.Category.Event.Date
            }).ToListAsync();
        return Ok(list);
    }

    // Participant cancels own enrolment
    [HttpDelete("api/enrolments/{id}")]
    [SessionAuthorize("Participant")]
    public async Task<IActionResult> Cancel(int id)
    {
        var e = await _db.EventEnrolments.FindAsync(id);
        if (e == null) return NotFound(new { message = "Enrolment not found." });
        if (e.ParticipantId != HttpContext.Session.GetInt32("UserId"))
            return StatusCode(403, new { message = "You can only cancel your own enrolments." });

        _db.EventEnrolments.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Organiser views enrolments for their event
    [HttpGet("api/events/{eventId}/enrolments")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            return StatusCode(403, new { message = "You can only view enrolments for your own events." });

        var list = await _db.EventEnrolments
            .Where(x => x.Category.EventId == eventId)
            .Select(x => new
            {
                x.Id,
                x.EnrolmentDate,
                Participant = x.Participant.Name,
                Category = x.Category.Name
            }).ToListAsync();
        return Ok(list);
    }
}