using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
public class ImagesController : ControllerBase
{
    private const long MaxBytes = 5 * 1024 * 1024;
    private static readonly Dictionary<string, string> Allowed = new()
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };
    private static readonly Regex NamePattern = new(@"^[a-f0-9]{32}\.(jpg|png|webp)$");

    private readonly RaceDayContext _db;
    private readonly IBlobStorageService _blobs;

    public ImagesController(RaceDayContext db, IBlobStorageService blobs)
    {
        _db = db;
        _blobs = blobs;
    }

    private static string? Validate(IFormFile? file)
    {
        if (file == null || file.Length == 0) return "Please choose an image file.";
        if (file.Length > MaxBytes) return "The image must be 5 MB or smaller.";
        if (!Allowed.ContainsKey(file.ContentType)) return "Only JPG, PNG or WEBP images are allowed.";
        return null;
    }

    private static string? NameFromUrl(string? url) =>
        url != null && url.StartsWith("/api/images/") ? url["/api/images/".Length..] : null;

    // Organiser uploads or replaces the banner of their own event
    [HttpPost("api/events/{id}/banner")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> UploadBanner(int id, IFormFile? file)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            return StatusCode(403, new { message = "You can only change the banner of your own events." });

        var problem = Validate(file);
        if (problem != null) return BadRequest(new { message = problem });

        using var stream = file!.OpenReadStream();
        var name = await _blobs.UploadAsync(stream, file.ContentType, Allowed[file.ContentType]);

        var old = NameFromUrl(ev.BannerUrl);
        ev.BannerUrl = $"/api/images/{name}";
        await _db.SaveChangesAsync();
        if (old != null) await _blobs.DeleteAsync(old);

        return Ok(new { ev.Id, ev.BannerUrl });
    }

    // Any logged in user uploads or replaces their own profile picture
    [HttpPost("api/users/me/picture")]
    [SessionAuthorize]
    public async Task<IActionResult> UploadPicture(IFormFile? file)
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound(new { message = "User not found." });

        var problem = Validate(file);
        if (problem != null) return BadRequest(new { message = problem });

        using var stream = file!.OpenReadStream();
        var name = await _blobs.UploadAsync(stream, file.ContentType, Allowed[file.ContentType]);

        var old = NameFromUrl(user.ProfilePictureUrl);
        user.ProfilePictureUrl = $"/api/images/{name}";
        await _db.SaveChangesAsync();
        if (old != null) await _blobs.DeleteAsync(old);

        return Ok(new { user.Id, user.ProfilePictureUrl });
    }

    // Serves a stored image (the website shows images through this endpoint)
    [HttpGet("api/images/{name}")]
    public async Task<IActionResult> Get(string name)
    {
        if (!NamePattern.IsMatch(name)) return NotFound();
        var image = await _blobs.DownloadAsync(name);
        if (image == null) return NotFound();

        Response.Headers.CacheControl = "public,max-age=3600";
        return File(image.Value.Content, image.Value.ContentType);
    }
}