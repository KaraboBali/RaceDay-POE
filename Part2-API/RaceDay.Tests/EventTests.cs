using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Tests;

public class EventTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public EventTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateEvent_AsOrganiser_ReturnsCreated()
    {
        var organiser = await Api.LoggedIn(_factory, "Organiser");

        var response = await Api.CreateEvent(organiser);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_AsParticipant_ReturnsForbidden()
    {
        var participant = await Api.LoggedIn(_factory, "Participant");

        var response = await Api.CreateEvent(participant);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WhenNotLoggedIn_ReturnsUnauthorized()
    {
        var anonymous = _factory.CreateClient();

        var response = await Api.CreateEvent(anonymous);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetEvents_WhenLoggedIn_ReturnsOk()
    {
        var participant = await Api.LoggedIn(_factory, "Participant");

        var response = await participant.GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetEvent_WithUnknownId_ReturnsNotFound()
    {
        var participant = await Api.LoggedIn(_factory, "Participant");

        var response = await participant.GetAsync("/api/events/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEvent_AsOwningOrganiser_ReturnsOk()
    {
        var organiser = await Api.LoggedIn(_factory, "Organiser");
        var id = await Api.IdOf(await Api.CreateEvent(organiser));

        var response = await organiser.PutAsJsonAsync($"/api/events/{id}", new
        {
            name = "Renamed Run",
            date = DateTime.UtcNow.AddDays(40),
            location = "Durban",
            description = "Updated",
            eventType = "Walk"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEvent_AsOwningOrganiser_ReturnsNoContent()
    {
        var organiser = await Api.LoggedIn(_factory, "Organiser");
        var id = await Api.IdOf(await Api.CreateEvent(organiser));

        var response = await organiser.DeleteAsync($"/api/events/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEvent_AsDifferentOrganiser_ReturnsForbidden()
    {
        var owner = await Api.LoggedIn(_factory, "Organiser");
        var id = await Api.IdOf(await Api.CreateEvent(owner));
        var otherOrganiser = await Api.LoggedIn(_factory, "Organiser");

        var response = await otherOrganiser.DeleteAsync($"/api/events/{id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
