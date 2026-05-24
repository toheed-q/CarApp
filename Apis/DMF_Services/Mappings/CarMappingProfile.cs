using AutoMapper;

using DMF_Services.DTOs.Cars;
using DMF_Services.DTOs.Common;
using DMF_Services.Models;

namespace DMF_Services.Mappings
{
    public class CarMappingProfile : Profile
    {
        public CarMappingProfile()
        {
            CreateMap<CarDetail, CarDetailDto>()
            .ForMember(dest => dest.Images, opt =>
                opt.MapFrom(src =>
                    src.CarImage == null ? null : new CarImageDto
                    {
                        Images = new List<string?>
                        {
                            src.CarImage.Image1,
                            src.CarImage.Image2,
                            src.CarImage.Image3,
                            src.CarImage.Image4,
                            src.CarImage.Image5,
                            src.CarImage.Image6,
                            src.CarImage.Image7,
                            src.CarImage.Image8,
                            src.CarImage.Image9,
                            src.CarImage.Image10,
                            src.CarImage.Image11,
                            src.CarImage.Image12,
                            src.CarImage.Image13,
                            src.CarImage.Image14,
                            src.CarImage.Image15,
                            src.CarImage.Image16,
                            src.CarImage.Image17,
                            src.CarImage.Image18,
                            src.CarImage.Image19,
                            src.CarImage.Image20
                        }
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!)
                        .ToList()
                    }))
            .ForMember(d => d.Location, o =>
                o.MapFrom(s =>
                    s.CarLocation == null ? null : new GeoLocationDto
                    {
                        Latitude = s.CarLocation.Y,
                        Longitude = s.CarLocation.X
                    }));
        }
    }
}
