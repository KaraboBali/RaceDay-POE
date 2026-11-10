using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.Services;

namespace RaceDay.Tests;

// Stands in for Azure Blob Storage during tests, so no cloud account is needed.
public class FakeBlobStorage : IBlobStorageService
{
    private readonly Dictionary<string, (byte[] Data, string Type)> _store = new();

    public async Task<string> UploadAsync(Stream content, string contentType, string extension)
    {
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms);
        var name = $"{Guid.NewGuid():N}{extension}";
        _store[name] = (ms.ToArray(), contentType);
        return name;
    }

    public Task<(Stream Content, string ContentType)?> DownloadAsync(string blobName)
    {
        if (!_store.TryGetValue(blobName, out var item))
            return Task.FromResult<(Stream Content, string ContentType)?>(null);
        return Task.FromResult<(Stream Content, string ContentType)?>(
            (new MemoryStream(item.Data), item.Type));
    }

    public Task DeleteAsync(string blobName)
    {
        _store.Remove(blobName);
        return Task.CompletedTask;
    }
}

// Starts the real API in memory, but swaps SQL Server for an in-memory database
// and Azure Blob Storage for the fake above.
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

            var blob = services.SingleOrDefault(s => s.ServiceType == typeof(IBlobStorageService));
            if (blob != null) services.Remove(blob);
            services.AddSingleton<IBlobStorageService, FakeBlobStorage>();
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