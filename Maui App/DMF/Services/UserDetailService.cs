using DMF.DTOs.User;

namespace DMF.Services
{
    public class UserDetailService : IUserDetailService
    {
        private readonly IApiService _apiService;

        public UserDetailService(IApiService apiService)
        {
            _apiService = apiService;
        }

        // ---------------- GET ALL ----------------
        public async Task<ApiResponse<IEnumerable<UserDetailDto>>> GetAllAsync()
        {
            return await _apiService.GetAsync<IEnumerable<UserDetailDto>>(
                "user-details");
        }

        // ---------------- GET BY ID ----------------
        public async Task<ApiResponse<UserDetailDto?>> GetByIdAsync(int id)
        {
            return await _apiService.GetAsync<UserDetailDto?>($"user-details/{id}");
        }

        // ---------------- GET BY MOBILE ----------------
        public async Task<ApiResponse<UserDetailDto?>> GetByMobileNoAsync(string mobile, bool isActive = true)
        {
            var endpoint =
                $"user-details/get-user-by-mobile-no" +
                $"?mobile={Uri.EscapeDataString(mobile)}" +
                $"&isActive={isActive}";

            return await _apiService.GetAsync<UserDetailDto?>(endpoint);
        }

        // ---------------- CREATE ----------------
        public async Task<ApiResponse<(UserDetailDto UserDetail, bool IsCreated)>> CreateAsync(UserDetailDto dto)
        {
            var response = await _apiService
                .PostAsync<UserDetailDto, UserDetailDto>("user-details", dto);

            if (!response.Success || response.Data == null)
            {
                return new ApiResponse<(UserDetailDto, bool)>
                {
                    Success = false,
                    Message = response.Message
                };
            }

            // Infer IsCreated from message (as per controller logic)
            bool isCreated =
                response.Message.Contains("created", StringComparison.OrdinalIgnoreCase);

            return new ApiResponse<(UserDetailDto, bool)>
            {
                Success = true,
                Message = response.Message,
                Data = (response.Data, isCreated)
            };
        }

        // ---------------- UPDATE ----------------
        public async Task<ApiResponse<bool>> UpdateAsync(int id, UserDetailDto dto)
        {
            var response = await _apiService
                .PutAsync<UserDetailDto, bool>($"user-details/{id}", dto);

            return response;
        }
    }
}
