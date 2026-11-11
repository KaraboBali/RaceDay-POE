using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class AccountController : AppController
{
    private readonly ApiClient _api;

    public AccountController(ApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (HttpContext.Session.IsLoggedIn())
        {
            return GoHome();
        }
        return View(new RegisterViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _api.PostAsync<object>("api/auth/register",
            new { model.Name, model.Email, model.Password, model.Role });

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        Flash(true, "Your account is ready. Please log in.");
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.IsLoggedIn())
        {
            return GoHome();
        }
        return View(new LoginViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        HttpContext.Session.Clear();
        var result = await _api.PostAsync<LoginInfo>("api/auth/login",
            new { model.Email, model.Password }, storeCookie: true);

        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError(string.Empty,
                result.StatusCode == 401 ? "Invalid email or password." : result.Message);
            return View(model);
        }

        HttpContext.Session.SignIn(result.Data.Id, result.Data.Name, result.Data.Role);
        Flash(true, $"Welcome back, {result.Data.Name}!");
        return GoHome();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _api.PostAsync<object>("api/auth/logout");
        HttpContext.Session.Clear();
        Flash(true, "You have been logged out.");
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}