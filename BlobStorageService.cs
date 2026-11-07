
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;

namespace Agristream.Services;

public class BlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public BlobStorageService(IConfiguration configuration)
    {
        string serviceUri = configuration["BlobStorage:ServiceUri"]
            ?? throw new InvalidOperationException(
                "Blob Storage Service URI is missing.");

        string containerName = configuration["BlobStorage:ContainerName"]
            ?? throw new InvalidOperationException(
                "Blob Storage container name is missing.");

        var blobServiceClient = new BlobServiceClient(
            new Uri(serviceUri),
            new DefaultAzureCredential());

        _containerClient = blobServiceClient
            .GetBlobContainerClient(containerName);
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        string extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        string blobName = $"{Guid.NewGuid()}{extension}";

        BlobClient blobClient = _containerClient.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();

        await blobClient.UploadAsync(stream, overwrite: false);

        return blobName;
    }

    public async Task DeleteImageAsync(string blobName)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync();
    }

    public async Task<Stream> DownloadImageAsync(string blobName)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(blobName);

        var response = await blobClient.DownloadStreamingAsync();

        return response.Value.Content;
    }
}
