using Azure.Storage.Blobs;

namespace RaceDay.Api.Services;

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream content, string contentType, string extension);
    Task<(Stream Content, string ContentType)?> DownloadAsync(string blobName);
    Task DeleteAsync(string blobName);
}

// Talks to Azure Blob Storage (or the Azurite local emulator).
// The connection string comes from appsettings.json, so swapping in a real Azure account is one setting.
public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _container;
    private bool _ready;

    public BlobStorageService(IConfiguration config)
    {
        var connection = config["BlobStorage:ConnectionString"] ?? "UseDevelopmentStorage=true";
        var containerName = config["BlobStorage:Container"] ?? "raceday-images";
        _container = new BlobContainerClient(connection, containerName);
    }

    private async Task EnsureContainerAsync()
    {
        if (_ready) return;
        await _container.CreateIfNotExistsAsync();
        _ready = true;
    }

    public async Task<string> UploadAsync(Stream content, string contentType, string extension)
    {
        await EnsureContainerAsync();
        var name = $"{Guid.NewGuid():N}{extension}";
        var blob = _container.GetBlobClient(name);
        await blob.UploadAsync(content, new Azure.Storage.Blobs.Models.BlobUploadOptions
        {
            HttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = contentType }
        });
        return name;
    }

    public async Task<(Stream Content, string ContentType)?> DownloadAsync(string blobName)
    {
        await EnsureContainerAsync();
        var blob = _container.GetBlobClient(blobName);
        if (!await blob.ExistsAsync()) return null;
        var response = await blob.DownloadStreamingAsync();
        return (response.Value.Content, response.Value.Details.ContentType);
    }

    public async Task DeleteAsync(string blobName)
    {
        await EnsureContainerAsync();
        await _container.GetBlobClient(blobName).DeleteIfExistsAsync();
    }
}