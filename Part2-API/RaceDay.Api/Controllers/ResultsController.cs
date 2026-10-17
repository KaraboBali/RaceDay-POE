using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
public class ResultsController : ControllerBase
{
    private readonly RaceDayContext _db;
    public ResultsController(RaceDayContext db) => _db = db;

    // Organiser records a result for an enrolment in their own event
    [HttpPost("api/enrolments/{enrolmentId}/result")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Record(int enrolmentId, ResultDto dto)
    {
        var enrolment = await _db.EventEnrolments
            .Include(x => x.Category).ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(x => x.Id == enrolmentId);
        if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
        if (enrolment.Category.Event.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            return StatusCode(403, new { message = "You can only record results for your own events." });
        if (dto.Position < 1 || dto.FinishTime <= TimeSpan.Zero)
            return BadRequest(new { message = "Position must be 1 or more and finish time must be positive." });
        if (await _db.Results.AnyAsync(r => r.EnrolmentId == enrolmentId))
            return Conflict(new { message = "A result already exists for this enrolment." });

        var result = new Result { EnrolmentId = enrolmentId, FinishTime = dto.FinishTime, Position = dto.Position };
        _db.Results.Add(result);
        await _db.SaveChangesAsync();
        return StatusCode(201, new { result.Id, result.EnrolmentId, result.FinishTime, result.Position });
    }

    // Organiser corrects a result
    [HttpPut("api/results/{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Update(int id, ResultDto dto)
    {
        var result = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e.Category).ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (result == null) return NotFound(new { message = "Result not found." });
        if (result.Enrolment.Category.Event.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            return StatusCode(403, new { message = "You can only edit results for your own events." });

        result.FinishTime = dto.FinishTime;
        result.Position = dto.Position;
        await _db.SaveChangesAsync();
        return Ok(new { result.Id, result.EnrolmentId, result.FinishTime, result.Position });
    }

    // Organiser views results for their event
    [HttpGet("api/events/{eventId}/results")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            return StatusCode(403, new { message = "You can only view results for your own events." });

        var list = await _db.Results
            .Where(r => r.Enrolment.Category.EventId == eventId)
            .OrderBy(r => r.Position)
            .Select(r => new
            {
                r.Id,
                r.Position,
                r.FinishTime,
                Participant = r.Enrolment.Participant.Name,
                Category = r.Enrolment.Category.Name
            }).ToListAsync();
        return Ok(list);
    }

    // Participant views only their own results
    [HttpGet("api/results/mine")]
    [SessionAuthorize("Participant")]
    public async Task<IActionResult> Mine()
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var list = await _db.Results
            .Where(r => r.Enrolment.ParticipantId == userId)
            .Select(r => new
            {
                r.Id,
                r.Position,
                r.FinishTime,
                Event = r.Enrolment.Category.Event.Name,
                Category = r.Enrolment.Category.Name
            }).ToListAsync();
        return Ok(list);
    }
}
