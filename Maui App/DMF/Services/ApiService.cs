using System.Net.Http.Json;

namespace DMF
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // -------------------- GET --------------------
        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                    return new ApiResponse<T> { Success = false, Message = "API call failed" };

                var raw = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<T>>(
                    raw,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result ?? new ApiResponse<T> { Success = false, Message = "Empty response" };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // -------------------- POST --------------------
        public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, request);

                var rawContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"POST {endpoint} => {(int)response.StatusCode} | {rawContent}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<TResponse>
                    {
                        Success = false,
                        Message = $"API call failed ({(int)response.StatusCode}): {rawContent}"
                    };
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<TResponse>>(
                    rawContent,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result ?? new ApiResponse<TResponse>
                {
                    Success = false,
                    Message = "Empty response"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
                throw;
            }
        }

        // -------------------- PUT --------------------
        public async Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(
            string endpoint, TRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(endpoint, request);

                var rawContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"PUT {endpoint} => {(int)response.StatusCode} | {rawContent}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<TResponse>
                    {
                        Success = false,
                        Message = $"API call failed ({(int)response.StatusCode}): {rawContent}"
                    };
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<TResponse>>(
                    rawContent,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result ?? new ApiResponse<TResponse>
                {
                    Success = false,
                    Message = "Empty response"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in PutAsync: " + ex.ToString());
                throw;
            }
        }

        // -------------------- DELETE --------------------
        public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<T>
                    {
                        Success = false,
                        Message = "API call failed"
                    };
                }

                var result = await response.Content
                                           .ReadFromJsonAsync<ApiResponse<T>>(
                                               new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result ?? new ApiResponse<T>
                {
                    Success = false,
                    Message = "Empty response"
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
