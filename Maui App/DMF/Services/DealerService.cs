using DMF.DTOs.Dealer;
using DMF.Services.Interfaces;

namespace DMF.Services
{
    public class DealerService : IDealerService
    {
        private readonly IApiService _apiService;

        public DealerService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<ApiResponse<DealerRequestDto>> SubmitRequestAsync(CreateDealerRequestDto dto)
        {
            return await _apiService
                .PostAsync<CreateDealerRequestDto, DealerRequestDto>("dealer-requests", dto);
        }
    }
}
