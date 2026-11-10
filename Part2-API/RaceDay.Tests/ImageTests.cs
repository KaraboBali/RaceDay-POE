using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Xunit;

namespace RaceDay.Tests;

public class ImageTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public ImageTests(ApiFactory factory) => _factory = factory;

    private static readonly byte[] TinyImage = { 1, 2, 3, 4, 5 };

    private static MultipartFormDataContent Form(string contentType)
    {
        var file = new ByteArrayContent(TinyImage);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { file, "file", "picture" } };
    }

    private static async Task<string> ReadString(HttpResponseMessage r, string property)
    {
        using var doc = JsonDocument.Parse(await r.Content.ReadAsStringAsync());
        foreach (var p in doc.RootElement.EnumerateObject())
            if (string.Equals(p.Name, property, StringComparison.OrdinalIgnoreCase))
                return p.Value.GetString()!;
        throw new InvalidOperationException("Missing " + property);
    }

    [Fact]
    public async Task UploadBanner_AsOwningOrganiser_ReturnsOkWithUrl()
    {
        var organiser = await Api.LoggedIn(_factory, "Organiser");
        var id = await Api.IdOf(await Api.CreateEvent(organiser));

        var response = await organiser.PostAsync($"/api/events/{id}/banner", Form("image/png"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("/api/images/", await ReadString(response, "bannerUrl"));
    }

    [Fact]
    public async Task UploadBanner_AsParticipant_ReturnsForbidden()
    {
        var organiser = await Api.LoggedIn(_factory, "Organiser");
        var id = await Api.IdOf(await Api.CreateEvent(organiser));
        var participant = await Api.LoggedIn(_factory, "Participant");

        var response = await participant.PostAsync($"/api/events/{id}/banner", Form("image/png"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UploadBanner_AsDifferentOrganiser_ReturnsForbidden()
    {
        var owner = await Api.LoggedIn(_factory, "Organiser");
        var id = await Api.IdOf(await Api.CreateEvent(owner));
        var other = await Api.LoggedIn(_factory, "Organiser");

        var response = await other.PostAsync($"/api/events/{id}/banner", Form("image/png"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UploadBanner_WithWrongFileType_ReturnsBadRequest()
    {
        var organiser = await Api.LoggedIn(_factory, "Organiser");
        var id = await Api.IdOf(await Api.CreateEvent(organiser));

        var response = await organiser.PostAsync($"/api/events/{id}/banner", Form("text/plain"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadBanner_WhenNotLoggedIn_ReturnsUnauthorized()
    {
        var anonymous = _factory.CreateClient();

        var response = await anonymous.PostAsync("/api/events/1/banner", Form("image/png"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UploadProfilePicture_AsParticipant_ReturnsOk()
    {
        var participant = await Api.LoggedIn(_factory, "Participant");

        var response = await participant.PostAsync("/api/users/me/picture", Form("image/jpeg"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetImage_AfterUpload_ReturnsTheImage()
    {
        var participant = await Api.LoggedIn(_factory, "Participant");
        var upload = await participant.PostAsync("/api/users/me/picture", Form("image/png"));
        var url = await ReadString(upload, "profilePictureUrl");

        var response = await _factory.CreateClient().GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetImage_WithInvalidName_ReturnsNotFound()
    {
        var response = await _factory.CreateClient().GetAsync("/api/images/not-a-real-image.exe");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}