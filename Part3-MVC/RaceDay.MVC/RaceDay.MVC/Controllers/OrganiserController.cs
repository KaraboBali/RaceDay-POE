using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

[RoleAuthorize("Organiser")]
public class OrganiserController : AppController
{
    private readonly ApiClient _api;

    public OrganiserController(ApiClient api)
    {
        _api = api;
    }

    // ---------- helpers ----------

    private Task<ApiResult<List<EventItem>>> MyEvents()
    {
        return _api.GetAsync<List<EventItem>>($"api/events?organiserId={HttpContext.Session.CurrentUserId()}");
    }

    private async Task<EventItem?> LoadMyEvent(int eventId)
    {
        var result = await _api.GetAsync<EventItem>($"api/events/{eventId}");
        if (!result.Success || result.Data == null) return null;
        if (result.Data.OrganiserId != HttpContext.Session.CurrentUserId()) return null;
        return result.Data;
    }

    private IActionResult EventNotFound()
    {
        Flash(false, "We could not find that event.");
        return RedirectToAction(nameof(Events));
    }

    private static object EventBody(EventFormViewModel m)
    {
        return new { m.Name, m.Date, m.Location, m.Description, m.EventType, m.DistanceKm };
    }

    // ---------- dashboard and events ----------

    public async Task<IActionResult> Dashboard()
    {
        var result = await MyEvents();
        if (result.StatusCode == 401) return SessionExpired();
        if (!result.Success) Flash(false, result.Message);
        return View(result.Data ?? new List<EventItem>());
    }

    public async Task<IActionResult> Events()
    {
        var result = await MyEvents();
        if (result.StatusCode == 401) return SessionExpired();
        if (!result.Success) Flash(false, result.Message);
        return View(result.Data ?? new List<EventItem>());
    }

    [HttpGet]
    public IActionResult CreateEvent()
    {
        return View("EventForm", new EventFormViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(EventFormViewModel model)
    {
        if (!ModelState.IsValid) return View("EventForm", model);

        var result = await _api.PostAsync<EventItem>("api/events", EventBody(model));
        if (result.StatusCode == 401) return SessionExpired();
        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View("EventForm", model);
        }

        if (model.BannerFile != null && model.BannerFile.Length > 0)
        {
            var upload = await _api.UploadAsync<object>($"api/events/{result.Data.Id}/banner", model.BannerFile);
            if (!upload.Success)
            {
                Flash(false, "Event created, but the banner was not saved: " + upload.Message);
                return RedirectToAction(nameof(Categories), new { eventId = result.Data.Id });
            }
        }

        Flash(true, "Event created. Now add the categories people can enrol in.");
        return RedirectToAction(nameof(Categories), new { eventId = result.Data.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        var ev = await LoadMyEvent(id);
        if (ev == null) return EventNotFound();

        return View("EventForm", new EventFormViewModel
        {
            Id = ev.Id,
            Name = ev.Name,
            Date = ev.Date,
            Location = ev.Location,
            EventType = ev.EventType,
            DistanceKm = ev.DistanceKm,
            Description = ev.Description,
            ExistingBannerUrl = ev.BannerUrl
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(int id, EventFormViewModel model)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            model.ExistingBannerUrl = (await LoadMyEvent(id))?.BannerUrl;
            return View("EventForm", model);
        }

        var result = await _api.PutAsync<EventItem>($"api/events/{id}", EventBody(model));
        if (result.StatusCode == 401) return SessionExpired();
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            model.ExistingBannerUrl = (await LoadMyEvent(id))?.BannerUrl;
            return View("EventForm", model);
        }

        if (model.BannerFile != null && model.BannerFile.Length > 0)
        {
            var upload = await _api.UploadAsync<object>($"api/events/{id}/banner", model.BannerFile);
            if (!upload.Success)
            {
                Flash(false, "Event saved, but the banner was not updated: " + upload.Message);
                return RedirectToAction(nameof(Events));
            }
        }

        Flash(true, "Event updated.");
        return RedirectToAction(nameof(Events));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var result = await _api.DeleteAsync($"api/events/{id}");
        if (result.StatusCode == 401) return SessionExpired();
        Flash(result.Success, result.Success ? "Event deleted." : result.Message);
        return RedirectToAction(nameof(Events));
    }

    // ---------- categories ----------

    public async Task<IActionResult> Categories(int eventId)
    {
        var ev = await LoadMyEvent(eventId);
        if (ev == null) return EventNotFound();

        var cats = await _api.GetAsync<List<CategoryItem>>($"api/events/{eventId}/categories");
        return View(new EventCategoriesViewModel
        {
            Event = ev,
            Categories = cats.Data ?? new List<CategoryItem>()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(int eventId, string? name, decimal distanceKm)
    {
        if (string.IsNullOrWhiteSpace(name) || distanceKm <= 0)
        {
            Flash(false, "Please enter a category name and a distance greater than 0.");
            return RedirectToAction(nameof(Categories), new { eventId });
        }

        var result = await _api.PostAsync<CategoryItem>($"api/events/{eventId}/categories",
            new { Name = name.Trim(), DistanceKm = distanceKm });
        if (result.StatusCode == 401) return SessionExpired();
        Flash(result.Success, result.Success ? "Category added." : result.Message);
        return RedirectToAction(nameof(Categories), new { eventId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCategory(int eventId, int id, string? name, decimal distanceKm)
    {
        if (string.IsNullOrWhiteSpace(name) || distanceKm <= 0)
        {
            Flash(false, "Please enter a category name and a distance greater than 0.");
            return RedirectToAction(nameof(Categories), new { eventId });
        }

        var result = await _api.PutAsync<CategoryItem>($"api/categories/{id}",
            new { Name = name.Trim(), DistanceKm = distanceKm });
        if (result.StatusCode == 401) return SessionExpired();
        Flash(result.Success, result.Success ? "Category updated." : result.Message);
        return RedirectToAction(nameof(Categories), new { eventId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int eventId, int id)
    {
        var result = await _api.DeleteAsync($"api/categories/{id}");
        if (result.StatusCode == 401) return SessionExpired();
        Flash(result.Success, result.Success ? "Category deleted." : result.Message);
        return RedirectToAction(nameof(Categories), new { eventId });
    }

    // ---------- enrolments ----------

    public async Task<IActionResult> Enrolments(int eventId)
    {
        var ev = await LoadMyEvent(eventId);
        if (ev == null) return EventNotFound();

        var list = await _api.GetAsync<List<EventEnrolmentItem>>($"api/events/{eventId}/enrolments");
        if (!list.Success) Flash(false, list.Message);
        return View(new EventEnrolmentsViewModel
        {
            Event = ev,
            Enrolments = list.Data ?? new List<EventEnrolmentItem>()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int eventId, int id, string status)
    {
        var result = await _api.PutAsync<object>($"api/enrolments/{id}/status", new { Status = status });
        if (result.StatusCode == 401) return SessionExpired();
        Flash(result.Success, result.Success ? $"Enrolment marked as {status}." : result.Message);
        return RedirectToAction(nameof(Enrolments), new { eventId });
    }

    // ---------- results ----------

    public async Task<IActionResult> Results(int eventId)
    {
        var ev = await LoadMyEvent(eventId);
        if (ev == null) return EventNotFound();

        var list = await _api.GetAsync<List<EventEnrolmentItem>>($"api/events/{eventId}/enrolments");
        if (!list.Success) Flash(false, list.Message);
        return View(new EventEnrolmentsViewModel
        {
            Event = ev,
            Enrolments = list.Data ?? new List<EventEnrolmentItem>()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResult(int eventId, int enrolmentId, int resultId, string? finishTime, int position)
    {
        var ev = await LoadMyEvent(eventId);
        if (ev == null) return EventNotFound();

        if (ev.Date > DateTime.Now)
        {
            Flash(false, "Results can only be published after the event has taken place.");
            return RedirectToAction(nameof(Results), new { eventId });
        }

        if (!DisplayHelpers.TryParseTime(finishTime, out var time))
        {
            Flash(false, "Enter the finish time like 1:23:45 (hours:minutes:seconds) or 23:45 (minutes:seconds).");
            return RedirectToAction(nameof(Results), new { eventId });
        }

        if (position < 1)
        {
            Flash(false, "The finishing position must be 1 or more.");
            return RedirectToAction(nameof(Results), new { eventId });
        }

        var body = new { FinishTime = time, Position = position };
        var result = resultId > 0
            ? await _api.PutAsync<object>($"api/results/{resultId}", body)
            : await _api.PostAsync<object>($"api/enrolments/{enrolmentId}/result", body);

        if (result.StatusCode == 401) return SessionExpired();
        Flash(result.Success, result.Success ? "Result saved." : result.Message);
        return RedirectToAction(nameof(Results), new { eventId });
    }
}