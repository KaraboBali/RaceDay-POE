using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

[RoleAuthorize]
public class EventsController : AppController
{
    private readonly ApiClient _api;

    public EventsController(ApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Details(int id)
    {
        var ev = await _api.GetAsync<EventItem>($"api/events/{id}");
        if (ev.StatusCode == 401) return SessionExpired();
        if (!ev.Success || ev.Data == null)
        {
            Flash(false, ev.Message);
            return GoHome();
        }

        var cats = await _api.GetAsync<List<CategoryItem>>($"api/events/{id}/categories");

        var vm = new EventDetailsViewModel
        {
            Event = ev.Data,
            Categories = cats.Data ?? new List<CategoryItem>(),
            IsParticipant = HttpContext.Session.CurrentRole() == "Participant"
        };

        if (vm.IsParticipant)
        {
            var mine = await _api.GetAsync<List<MyEnrolmentItem>>("api/enrolments/mine");
            vm.MyEnrolment = mine.Data?.FirstOrDefault(x => x.EventId == id);
        }

        return View(vm);
    }
}