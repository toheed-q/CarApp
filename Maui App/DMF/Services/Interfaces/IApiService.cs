namespace DMF.Services.Interfaces
{
    public interface IApiService
    {
        Task<ApiResponse<T>> GetAsync<T>(string endpoint);
        Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request);
        Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest request);
        Task<ApiResponse<T>> DeleteAsync<T>(string endpoint);
    }
}
