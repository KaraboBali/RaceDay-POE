using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;

namespace RaceDay.Tests;

// Starts the real API in memory, but swaps SQL Server for an in-memory database.
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var old = services.SingleOrDefault(
                s => s.ServiceType == typeof(DbContextOptions<RaceDayContext>));
            if (old != null) services.Remove(old);

            services.AddDbContext<RaceDayContext>(o => o.UseInMemoryDatabase(_dbName));
        });
    }
}

public static class Api
{
    public const string Password = "Pass123!";

    public static string NewEmail() => $"{Guid.NewGuid():N}@test.com";

    public static Task<HttpResponseMessage> Register(
        HttpClient c, string email, string role = "Participant", string password = Password)
        => c.PostAsJsonAsync("/api/auth/register",
            new { name = "Test User", email, password, role });

    public static Task<HttpResponseMessage> Login(
        HttpClient c, string email, string password = Password)
        => c.PostAsJsonAsync("/api/auth/login", new { email, password });

    // Registers a new user, logs them in, and returns a client holding their session cookie.
    public static async Task<HttpClient> LoggedIn(ApiFactory f, string role)
    {
        var c = f.CreateClient();
        var email = NewEmail();
        await Register(c, email, role);
        await Login(c, email);
        return c;
    }

    public static async Task<int> IdOf(HttpResponseMessage r)
    {
        var json = await r.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        foreach (var p in doc.RootElement.EnumerateObject())
            if (string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase))
                return p.Value.GetInt32();
        throw new InvalidOperationException("No id in response: " + json);
    }

    public static Task<HttpResponseMessage> CreateEvent(HttpClient c, string name = "Test Run")
        => c.PostAsJsonAsync("/api/events", new
        {
            name,
            date = DateTime.UtcNow.AddDays(30),
            location = "Cape Town",
            description = "Test event",
            eventType = "Run"
        });

    public static Task<HttpResponseMessage> CreateCategory(HttpClient c, int eventId)
        => c.PostAsJsonAsync($"/api/events/{eventId}/categories",
            new { name = "10km", distanceKm = 10 });
}
