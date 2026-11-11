using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

// Shows images by fetching them from the API, so the browser never talks to Azure.
public class ImageController : Controller
{
    private static readonly Regex Pattern = new(@"^/api/images/[a-f0-9]{32}\.(jpg|png|webp)$");
    private readonly ApiClient _api;

    public ImageController(ApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Show(string? url)
    {
        if (string.IsNullOrEmpty(url) || !Pattern.IsMatch(url))
        {
            return NotFound();
        }

        var image = await _api.GetImageAsync(url.TrimStart('/'));
        if (image == null)
        {
            return NotFound();
        }

        Response.Headers.CacheControl = "public,max-age=3600";
        return File(image.Value.Data, image.Value.ContentType);
    }
}