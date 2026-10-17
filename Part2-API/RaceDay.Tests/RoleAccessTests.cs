using System.Net;
using Xunit;

namespace RaceDay.Tests;

public class RoleAccessTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public RoleAccessTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task ViewUserById_AsParticipant_ReturnsForbidden()
    {
        var participant = await Api.LoggedIn(_factory, "Participant");

        var response = await participant.GetAsync("/api/users/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ViewUserById_AsOrganiser_IsAllowed()
    {
        var organiser = await Api.LoggedIn(_factory, "Organiser");

        var response = await organiser.GetAsync("/api/users/99999");

        // Role check passes, so the user simply isn't found (not 401 or 403).
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ViewEventEnrolments_AsParticipant_ReturnsForbidden()
    {
        var organiser = await Api.LoggedIn(_factory, "Organiser");
        var eventId = await Api.IdOf(await Api.CreateEvent(organiser));
        var participant = await Api.LoggedIn(_factory, "Participant");

        var response = await participant.GetAsync($"/api/events/{eventId}/enrolments");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ViewMyEnrolments_WhenNotLoggedIn_ReturnsUnauthorized()
    {
        var anonymous = _factory.CreateClient();

        var response = await anonymous.GetAsync("/api/enrolments/mine");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AfterLogout_ProtectedEndpoint_ReturnsUnauthorized()
    {
        var participant = await Api.LoggedIn(_factory, "Participant");

        await participant.PostAsync("/api/auth/logout", null);
        var response = await participant.GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}