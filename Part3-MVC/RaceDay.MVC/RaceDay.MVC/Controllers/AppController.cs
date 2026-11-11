using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

// Shared helpers for all controllers.
public abstract class AppController : Controller
{
    protected IActionResult GoHome() => RedirectToAction("Index", "Home");

    // The API forgot the login (for example after a restart), so ask the user to log in again.
    protected IActionResult SessionExpired()
    {
        HttpContext.Session.Clear();
        TempData["Error"] = "Your session has expired. Please log in again.";
        return RedirectToAction("Login", "Account");
    }

    protected void Flash(bool success, string message) =>
        TempData[success ? "Success" : "Error"] = message;
}