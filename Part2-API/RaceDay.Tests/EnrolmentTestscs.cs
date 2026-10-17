using System.Net;
using Xunit;

namespace RaceDay.Tests;

public class EnrolmentTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public EnrolmentTests(ApiFactory factory) => _factory = factory;

    // Organiser creates an event with one category and returns the category id.
    private async Task<int> SetupCategory()
    {
        var organiser = await Api.LoggedIn(_factory, "Organiser");
        var eventId = await Api.IdOf(await Api.CreateEvent(organiser));
        return await Api.IdOf(await Api.CreateCategory(organiser, eventId));
    }

    [Fact]
    public async Task Enrol_AsParticipant_ReturnsCreated()
    {
        var categoryId = await SetupCategory();
        var participant = await Api.LoggedIn(_factory, "Participant");

        var response = await participant.PostAsync(
            $"/api/categories/{categoryId}/enrolments", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_Twice_ReturnsConflict()
    {
        var categoryId = await SetupCategory();
        var participant = await Api.LoggedIn(_factory, "Participant");
        await participant.PostAsync($"/api/categories/{categoryId}/enrolments", null);

        var response = await participant.PostAsync(
            $"/api/categories/{categoryId}/enrolments", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_AsOrganiser_ReturnsForbidden()
    {
        var categoryId = await SetupCategory();
        var organiser = await Api.LoggedIn(_factory, "Organiser");

        var response = await organiser.PostAsync(
            $"/api/categories/{categoryId}/enrolments", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_InUnknownCategory_ReturnsNotFound()
    {
        var participant = await Api.LoggedIn(_factory, "Participant");

        var response = await participant.PostAsync(
            "/api/categories/99999/enrolments", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MyEnrolments_AsParticipant_ReturnsOk()
    {
        var categoryId = await SetupCategory();
        var participant = await Api.LoggedIn(_factory, "Participant");
        await participant.PostAsync($"/api/categories/{categoryId}/enrolments", null);

        var response = await participant.GetAsync("/api/enrolments/mine");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CancelEnrolment_AsOwner_ReturnsNoContent()
    {
        var categoryId = await SetupCategory();
        var participant = await Api.LoggedIn(_factory, "Participant");
        var enrol = await participant.PostAsync(
            $"/api/categories/{categoryId}/enrolments", null);
        var enrolmentId = await Api.IdOf(enrol);

        var response = await participant.DeleteAsync($"/api/enrolments/{enrolmentId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}