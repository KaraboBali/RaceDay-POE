using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Filters;

namespace RaceDay.Api.Controllers;

[ApiController]
public class UsersController : ControllerBase
{
    private readonly RaceDayContext _db;
    public UsersController(RaceDayContext db) => _db = db;

    // Both roles: view own profile
    [HttpGet("api/users/me")]
    [SessionAuthorize]
    public async Task<IActionResult> Me()
    {
        var id = HttpContext.Session.GetInt32("UserId")!.Value;
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound(new { message = "User not found." });
        return Ok(new { u.Id, u.Name, u.Email, u.Role });
    }

    // Both roles: update own profile
    [HttpPut("api/users/me")]
    [SessionAuthorize]
    public async Task<IActionResult> UpdateMe(ProfileDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { message = "Name and email are required." });

        var id = HttpContext.Session.GetInt32("UserId")!.Value;
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound(new { message = "User not found." });

        if (await _db.Users.AnyAsync(x => x.Email == dto.Email && x.Id != id))
            return Conflict(new { message = "Email already in use." });

        u.Name = dto.Name;
        u.Email = dto.Email;
        await _db.SaveChangesAsync();
        return Ok(new { u.Id, u.Name, u.Email, u.Role });
    }

    // Organiser: view a user (from your endpoint plan)
    [HttpGet("api/users/{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Get(int id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound(new { message = "User not found." });
        return Ok(new { u.Id, u.Name, u.Email, u.Role });
    }
}