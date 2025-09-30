using Application.Interfaces;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;

namespace Persistence.External;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _serviceClient;
    private readonly BlobStorageOptions _opts;

    public BlobStorageService(BlobServiceClient serviceClient, IOptions<BlobStorageOptions> opts)
    {
        _serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
        _opts = opts?.Value ?? new BlobStorageOptions();
    }

    public async Task DeleteFileAsync(
        string containerName,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var blob = await GetBlobClientAsync(containerName, fileName, createIfMissing: false, cancellationToken);
        if (blob is null) return;

        try
        {
            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, conditions: null, cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // already gone — ignore
        }
    }

    public async Task<Uri> GetFileUriAsync(
        string containerName,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var blob = await GetBlobClientAsync(containerName, fileName, createIfMissing: false, cancellationToken);
        if (blob is null)
            throw new FileNotFoundException($"Blob '{fileName}' not found in container '{containerName}'.");

        // If container is public (Blob access) and SAS is not required, return direct URL
        if (!_opts.UseSasUrls)
            return blob.Uri;

        // Generate a read-only SAS if we can sign requests
        if (blob.CanGenerateSasUri)
            return GenerateReadSas(blob, _opts.SasTtl);

        // Fallback: direct URL (will only work if container is public)
        return blob.Uri;
    }

    public async Task<Uri> UploadFileAsync(
        string containerName,
        string fileName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

        var blob = await GetBlobClientAsync(containerName, fileName, createIfMissing: true, cancellationToken);

        var headers = new BlobHttpHeaders { ContentType = contentType ?? "application/octet-stream" };

        // Upload (overwrite if exists)
        await blob.UploadAsync(fileStream, new BlobUploadOptions
        {
            HttpHeaders = headers
        }, cancellationToken);

        // Return SAS URL when configured (useful for private containers)
        if (_opts.UseSasUrls && blob.CanGenerateSasUri)
            return GenerateReadSas(blob, _opts.SasTtl);

        return blob.Uri;
    }

    // ---------- helpers ----------

    private async Task<BlobClient?> GetBlobClientAsync(
        string containerName,
        string fileName,
        bool createIfMissing,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(containerName))
            throw new ArgumentException("Container name is required.", nameof(containerName));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.", nameof(fileName));

        var container = _serviceClient.GetBlobContainerClient(containerName);

        if (createIfMissing)
        {
            // Create container if not exists; set public access per options (Blob = public read, None = private)
            await container.CreateIfNotExistsAsync(_opts.DefaultAccess, cancellationToken: ct);
        }
        else
        {
            var exists = await container.ExistsAsync(ct);
            if (!exists) return null;
        }

        return container.GetBlobClient(fileName);
    }

    private static Uri GenerateReadSas(BlobClient blob, TimeSpan ttl)
    {
        var sas = new BlobSasBuilder
        {
            BlobContainerName = blob.BlobContainerName,
            BlobName = blob.Name,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTimeOffset.UtcNow.Add(ttl)
        };
        sas.SetPermissions(BlobSasPermissions.Read);

        return blob.GenerateSasUri(sas);
    }
}
public sealed class BlobStorageOptions
{
    /// <summary>Default container access: Blob (public read) or None (private)</summary>
    public PublicAccessType DefaultAccess { get; set; } = PublicAccessType.None;

    /// <summary>Use SAS URLs when returning file URIs (recommended if private)</summary>
    public bool UseSasUrls { get; set; } = true;

    /// <summary>Default SAS lifetime</summary>
    public TimeSpan SasTtl { get; set; } = TimeSpan.FromHours(1);
}