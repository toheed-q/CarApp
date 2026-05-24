using AutoMapper;
using DMF_Services.DTOs.Cars;
using DMF_Services.Models;

namespace DMF_Services.Mappings
{
    public class CarLookupMappingProfile : Profile
    {
        public CarLookupMappingProfile()
        {
            CreateMap<CarBrandRaw, CarBrandDto>();
            CreateMap<CarModelRaw, CarModelDto>();
        }
    }
}
