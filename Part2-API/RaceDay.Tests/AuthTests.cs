using System.Net;
using Xunit;

namespace RaceDay.Tests;

public class AuthTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public AuthTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_WithValidDetails_ReturnsCreated()
    {
        var client = _factory.CreateClient();

        var response = await Api.Register(client, Api.NewEmail(), "Participant");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        var email = Api.NewEmail();
        await Api.Register(client, email);

        var response = await Api.Register(client, email);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithMissingEmail_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();

        var response = await Api.Register(client, "");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var email = Api.NewEmail();
        await Api.Register(client, email);

        var response = await Api.Login(client, email);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var email = Api.NewEmail();
        await Api.Register(client, email);

        var response = await Api.Login(client, email, "WrongPassword1!");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await Api.Login(client, Api.NewEmail());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}