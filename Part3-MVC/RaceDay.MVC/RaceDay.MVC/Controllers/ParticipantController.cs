using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

[RoleAuthorize("Participant")]
public class ParticipantController : AppController
{
    private readonly ApiClient _api;

    public ParticipantController(ApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> MyEnrolments()
    {
        var result = await _api.GetAsync<List<MyEnrolmentItem>>("api/enrolments/mine");
        if (result.StatusCode == 401) return SessionExpired();
        if (!result.Success) Flash(false, result.Message);
        return View(result.Data ?? new List<MyEnrolmentItem>());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int categoryId, int eventId)
    {
        var result = await _api.PostAsync<object>($"api/categories/{categoryId}/enrolments");
        if (result.StatusCode == 401) return SessionExpired();

        if (result.Success)
        {
            Flash(true, "You are enrolled! Your enrolment is pending until the organiser confirms it.");
            return RedirectToAction(nameof(MyEnrolments));
        }

        Flash(false, result.Message);
        return RedirectToAction("Details", "Events", new { id = eventId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _api.DeleteAsync($"api/enrolments/{id}");
        if (result.StatusCode == 401) return SessionExpired();
        Flash(result.Success, result.Success ? "Your enrolment was cancelled." : result.Message);
        return RedirectToAction(nameof(MyEnrolments));
    }

    public async Task<IActionResult> MyResults()
    {
        var result = await _api.GetAsync<List<MyResultItem>>("api/results/mine");
        if (result.StatusCode == 401) return SessionExpired();
        if (!result.Success) Flash(false, result.Message);
        return View(result.Data ?? new List<MyResultItem>());
    }
}