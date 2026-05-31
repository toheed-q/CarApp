namespace DMF.Services
{
    public class LocalUploadService : IBlobService
    {
        private readonly HttpClient _httpClient;

        public LocalUploadService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> UploadAsync(Stream stream, string fileName, string contentType)
        {
            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(streamContent, "file", Path.GetFileName(fileName));

            var response = await _httpClient.PostAsync("cars/upload-image", content);
            var raw = await response.Content.ReadAsStringAsync();

            var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<string>>(
                raw, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || !result.Success || result.Data == null)
                throw new Exception(result?.Message ?? "Upload failed");

            return result.Data;
        }

        public Task DeleteAsync(string url) => Task.CompletedTask;
    }
}
