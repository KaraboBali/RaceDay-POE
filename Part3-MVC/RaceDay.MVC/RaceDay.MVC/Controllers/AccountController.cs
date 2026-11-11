using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
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

    [RoleAuthorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var result = await _api.GetAsync<UserInfo>("api/users/me");
        if (result.StatusCode == 401) return SessionExpired();
        if (!result.Success || result.Data == null)
        {
            Flash(false, result.Message);
            return GoHome();
        }

        return View(new ProfileViewModel
        {
            Name = result.Data.Name,
            Email = result.Data.Email,
            Role = result.Data.Role,
            PictureUrl = result.Data.ProfilePictureUrl
        });
    }

    [RoleAuthorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await FillExtras(model);
            return View(model);
        }

        var result = await _api.PutAsync<UserInfo>("api/users/me", new { model.Name, model.Email });
        if (result.StatusCode == 401) return SessionExpired();

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await FillExtras(model);
            return View(model);
        }

        var session = HttpContext.Session;
        session.SignIn(session.CurrentUserId(), result.Data?.Name ?? model.Name, session.CurrentRole());
        Flash(true, "Your profile was updated.");
        return RedirectToAction(nameof(Profile));
    }

    [RoleAuthorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Picture(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            Flash(false, "Please choose an image file first.");
            return RedirectToAction(nameof(Profile));
        }

        var result = await _api.UploadAsync<object>("api/users/me/picture", file);
        if (result.StatusCode == 401) return SessionExpired();

        Flash(result.Success, result.Success ? "Your profile picture was updated." : result.Message);
        return RedirectToAction(nameof(Profile));
    }

    private async Task FillExtras(ProfileViewModel model)
    {
        var me = await _api.GetAsync<UserInfo>("api/users/me");
        model.PictureUrl = me.Data?.ProfilePictureUrl;
        model.Role = me.Data?.Role ?? string.Empty;
    }
}