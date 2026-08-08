using AutoMapper;
using DMF_Services.DTOs.DealerRequests;
using DMF_Services.Models;

namespace DMF_Services.Mappings
{
    public class DealerRequestMappingProfile : Profile
    {
        public DealerRequestMappingProfile()
        {
            CreateMap<DealerRequest, DealerRequestDto>();
            CreateMap<CreateDealerRequestDto, DealerRequest>()
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());
        }
    }
}
