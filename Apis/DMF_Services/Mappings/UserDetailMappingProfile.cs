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
            CreateMap<CreateUserDetailDto, UserDetail>();
            CreateMap<UpdateUserDetailDto, UserDetail>();
        }
    }
}
