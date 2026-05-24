using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace DMF.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobContainerClient _container;

        public BlobService(string connectionString, string containerName)
        {
            //var serviceClient = new BlobServiceClient(connectionString);
            //_container = serviceClient.GetBlobContainerClient(containerName);
        }

        // --------------------------------------
        // UPLOAD IMAGE
        // --------------------------------------
        public async Task<string> UploadAsync(Stream stream, string blobPath, string contentType)
        {
            var blobClient = _container.GetBlobClient(blobPath);

            await blobClient.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = contentType
            });

            return blobClient.Uri.ToString();
        }

        // --------------------------------------
        // DELETE IMAGE
        // --------------------------------------
        public async Task DeleteAsync(string blobUrl)
        {
            if (string.IsNullOrWhiteSpace(blobUrl))
                return;

            var blobName = GetBlobName(blobUrl);

            var blobClient = _container.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }

        // --------------------------------------
        // HELPER
        // --------------------------------------
        private string GetBlobName(string blobUrl)
        {
            var uri = new Uri(blobUrl);

            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // remove container name
            return string.Join('/', segments.Skip(1));
        }
    }
}