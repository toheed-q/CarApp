using DMF_Services.DTOs.DealerRequests;

namespace DMF_Services.Services.Interfaces
{
    public interface IDealerRequestService
    {
        Task<DealerRequestDto> CreateAsync(CreateDealerRequestDto dto);
    }
}
