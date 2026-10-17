using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayContext _db;
    public AuthController(RaceDayContext db) => _db = db;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Name, email and password are required." });

        if (dto.Role != "Organiser" && dto.Role != "Participant")
            return BadRequest(new { message = "Role must be Organiser or Participant." });

        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict(new { message = "Email already registered." });

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return StatusCode(201, new { user.Id, user.Name, user.Email, user.Role });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("Role", user.Role);

        return Ok(new { user.Id, user.Name, user.Role });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logged out." });
    }
}
