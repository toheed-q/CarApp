namespace DMF_Services.Services.Interfaces
{
    public interface IBlobStorageService
    {
        /// <summary>
        /// Uploads a file stream to Azure Blob Storage and returns its public URL.
        /// </summary>
        Task<string> UploadAsync(Stream stream, string fileName, string contentType);

        /// <summary>
        /// Deletes a blob given its full public URL. No-op if the URL is empty.
        /// </summary>
        Task DeleteAsync(string blobUrl);
    }
}
