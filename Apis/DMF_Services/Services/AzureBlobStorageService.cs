using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DMF_Services.Services.Interfaces;

namespace DMF_Services.Services
{
    /// <summary>
    /// Stores car images in Azure Blob Storage. The connection string lives only on
    /// the server (appsettings / user-secrets), so the storage key is never shipped
    /// to the mobile app.
    /// </summary>
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _container;
        private readonly ILogger<AzureBlobStorageService> _logger;

        public AzureBlobStorageService(IConfiguration config, ILogger<AzureBlobStorageService> logger)
        {
            _logger = logger;

            var connectionString = config["BlobStorage:ConnectionString"];
            var containerName = config["BlobStorage:Container"];

            if (string.IsNullOrWhiteSpace(connectionString) ||
                connectionString == "PASTE_YOUR_CONNECTION_STRING_HERE")
                throw new InvalidOperationException(
                    "BlobStorage:ConnectionString is not configured. Set it in appsettings.json or user-secrets.");

            var serviceClient = new BlobServiceClient(connectionString);
            _container = serviceClient.GetBlobContainerClient(containerName);

            // Create the container if it doesn't exist yet (safe if it already does).
            _container.CreateIfNotExists();
        }

        // --------------------------------------
        // UPLOAD  ->  returns the public blob URL
        // --------------------------------------
        public async Task<string> UploadAsync(Stream stream, string fileName, string contentType)
        {
            var blobClient = _container.GetBlobClient(fileName);

            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            });

            _logger.LogInformation("Uploaded blob {Blob} to container {Container}.",
                fileName, _container.Name);

            return blobClient.Uri.ToString();
        }

        // --------------------------------------
        // DELETE  (best-effort; never throws on a bad URL)
        // --------------------------------------
        public async Task DeleteAsync(string blobUrl)
        {
            if (string.IsNullOrWhiteSpace(blobUrl)) return;

            try
            {
                var blobName = GetBlobName(blobUrl);
                await _container.GetBlobClient(blobName).DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete blob from URL {Url}.", blobUrl);
            }
        }

        // Extracts the blob path (everything after the container name) from a full URL.
        private string GetBlobName(string blobUrl)
        {
            var uri = new Uri(blobUrl);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            // segments[0] is the container name -> skip it.
            return string.Join('/', segments.Skip(1));
        }
    }
}
