using AutoMapper;
using DMF_Services.DTOs.UserDetails;
using DMF_Services.Models;

namespace DMF_Services.Mappings
{
    public class UserDetailMappingProfile : Profile
    {
        public UserDetailMappingProfile()
        {
            CreateMap<UserDetail, UserDetailDto>();
            CreateMap<CreateUserDetailDto, UserDetail>()
                .ForMember(dest => dest.IsDealers, opt => opt.Ignore());
            CreateMap<UpdateUserDetailDto, UserDetail>()
                .ForMember(dest => dest.IsDealers, opt => opt.Ignore());
        }
    }
}
