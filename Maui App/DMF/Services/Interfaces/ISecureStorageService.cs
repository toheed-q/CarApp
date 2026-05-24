namespace DMF.Services.Interfaces
{
    public interface ISecureStorageService
    {
        Task SetAsync(string key, string value);
        Task<string?> GetAsync(string key);
        void Remove(string key);
    }
}
