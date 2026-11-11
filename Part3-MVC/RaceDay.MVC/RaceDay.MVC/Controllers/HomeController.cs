using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class HomeController : AppController
{
    private readonly ApiClient _api;

    public HomeController(ApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(string? type, string? search, bool includePast = false)
    {
        var session = HttpContext.Session;
        if (!session.IsLoggedIn() || session.CurrentRole() != "Participant")
        {
            return View();
        }

        var url = "api/events?upcomingOnly=" + (!includePast).ToString().ToLower();
        if (!string.IsNullOrWhiteSpace(type)) url += "&type=" + Uri.EscapeDataString(type);
        if (!string.IsNullOrWhiteSpace(search)) url += "&search=" + Uri.EscapeDataString(search);

        var result = await _api.GetAsync<List<EventItem>>(url);
        if (result.StatusCode == 401) return SessionExpired();
        if (!result.Success) Flash(false, result.Message);

        return View("Events", new EventListViewModel
        {
            Events = result.Data ?? new List<EventItem>(),
            Type = type,
            Search = search,
            IncludePast = includePast
        });
    }

    public IActionResult Error()
    {
        return View();
    }

    public IActionResult StatusPage(int code)
    {
        ViewData["Code"] = code;
        ViewData["Heading"] = code switch
        {
            404 => "Page not found",
            403 => "Access denied",
            _ => "Something went wrong"
        };
        return View();
    }
}