namespace DMF.Services.Interfaces
{
    public interface IBlobService
    {
        Task<string> UploadAsync(Stream stream, string fileName, string contentType);
        Task DeleteAsync(string blobUrl);
    }
}
