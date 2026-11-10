using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.MVC.Services;

public class ApiResult<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
}

// The only class that talks to the Part 2 API.
// The MVC project never touches the database or Azure directly.
public class ApiClient
{
    private const string CookieKey = "ApiCookie";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _context;

    public ApiClient(HttpClient http, IHttpContextAccessor context)
    {
        _http = http;
        _context = context;
    }

    private ISession Session => _context.HttpContext!.Session;

    // Sends the logged in user's API session cookie with every request.
    private HttpRequestMessage Build(HttpMethod method, string url, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        var cookie = Session.GetString(CookieKey);
        if (!string.IsNullOrEmpty(cookie))
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        return request;
    }

    private async Task<ApiResult<T>> SendAsync<T>(HttpRequestMessage request, bool storeCookie = false)
    {
        using var disposable = request;
        try
        {
            using var response = await _http.SendAsync(request);

            if (storeCookie && response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                var apiSession = cookies
                    .Select(c => c.Split(';')[0])
                    .FirstOrDefault(c => c.StartsWith(".AspNetCore.Session=", StringComparison.Ordinal));
                if (apiSession != null) Session.SetString(CookieKey, apiSession);
            }

            var body = await response.Content.ReadAsStringAsync();
            var result = new ApiResult<T>
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode
            };

            if (response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrWhiteSpace(body) && typeof(T) != typeof(object))
                    result.Data = JsonSerializer.Deserialize<T>(body, JsonOptions);
            }
            else
            {
                result.Message = ReadMessage(body, response.StatusCode);
            }
            return result;
        }
        catch (HttpRequestException)
        {
            return Failure<T>(503, "The RaceDay service is not reachable right now. Please try again in a moment.");
        }
        catch (TaskCanceledException)
        {
            return Failure<T>(504, "The RaceDay service took too long to respond. Please try again.");
        }
        catch (JsonException)
        {
            return Failure<T>(502, "The RaceDay service sent an unexpected reply. Please try again.");
        }
    }

    private static ApiResult<T> Failure<T>(int status, string message) =>
        new() { Success = false, StatusCode = status, Message = message };

    private static string ReadMessage(string body, HttpStatusCode status)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (doc.RootElement.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
                    return m.GetString() ?? string.Empty;
                if (doc.RootElement.TryGetProperty("errors", out _))
                    return "Some of the details you entered are not valid. Please check them and try again.";
            }
        }
        catch (JsonException) { }

        return status switch
        {
            HttpStatusCode.Unauthorized => "Please log in to continue.",
            HttpStatusCode.Forbidden => "You do not have permission to do that.",
            HttpStatusCode.NotFound => "We could not find what you were looking for.",
            HttpStatusCode.Conflict => "That action conflicts with existing information.",
            _ => "Something went wrong. Please try again."
        };
    }

    public Task<ApiResult<T>> GetAsync<T>(string url) =>
        SendAsync<T>(Build(HttpMethod.Get, url));

    public Task<ApiResult<T>> PostAsync<T>(string url, object? body = null, bool storeCookie = false) =>
        SendAsync<T>(Build(HttpMethod.Post, url,
            body == null ? null : JsonContent.Create(body, options: JsonOptions)), storeCookie);

    public Task<ApiResult<T>> PutAsync<T>(string url, object body) =>
        SendAsync<T>(Build(HttpMethod.Put, url, JsonContent.Create(body, options: JsonOptions)));

    public Task<ApiResult<object>> DeleteAsync(string url) =>
        SendAsync<object>(Build(HttpMethod.Delete, url));

    // Sends an image to the API, which stores it in Blob Storage.
    public Task<ApiResult<T>> UploadAsync<T>(string url, IFormFile file)
    {
        var form = new MultipartFormDataContent();
        var part = new StreamContent(file.OpenReadStream());
        part.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        form.Add(part, "file", file.FileName);
        return SendAsync<T>(Build(HttpMethod.Post, url, form));
    }

    // Fetches a stored image through the API so the browser never talks to Azure.
    public async Task<(byte[] Data, string ContentType)?> GetImageAsync(string url)
    {
        try
        {
            using var request = Build(HttpMethod.Get, url);
            using var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;
            var data = await response.Content.ReadAsByteArrayAsync();
            return (data, response.Content.Headers.ContentType?.MediaType ?? "image/jpeg");
        }
        catch (HttpRequestException) { return null; }
        catch (TaskCanceledException) { return null; }
    }
}