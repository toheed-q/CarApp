using DMF.DTOs.Dealer;

namespace DMF.Services.Interfaces
{
    public interface IDealerService
    {
        Task<ApiResponse<DealerRequestDto>> SubmitRequestAsync(CreateDealerRequestDto dto);
    }
}
