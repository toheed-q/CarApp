using AutoMapper;
using DMF_Services.DTOs.Cars;
using DMF_Services.DTOs.Common;
using DMF_Services.Models;

namespace DMF_Services.Mappings
{
    public class CarFilterMappingProfile : Profile
    {
        public CarFilterMappingProfile()
        {
            CreateMap<CarFilterRaw, CarFilterResultDto>()
                // -------- CarDetail --------
                .ForMember(d => d.ID, o => o.MapFrom(s => s.ID))
                .ForMember(d => d.DealersID, o => o.MapFrom(s => s.DealersID))
                .ForMember(d => d.Brand, o => o.MapFrom(s => s.Brand))
                .ForMember(d => d.Model, o => o.MapFrom(s => s.Model))
                .ForMember(d => d.Varient, o => o.MapFrom(s => s.Varient))
                .ForMember(d => d.Price, o => o.MapFrom(s => s.Price))
                .ForMember(d => d.RegistrationNo, o => o.MapFrom(s => s.RegistrationNo))
                .ForMember(d => d.RegistrationDate, o => o.MapFrom(s => s.RegistrationDate))
                .ForMember(d => d.KMDriven, o => o.MapFrom(s => s.KMDriven))
                .ForMember(d => d.Fuel, o => o.MapFrom(s => s.Fuel))
                .ForMember(d => d.Transmission, o => o.MapFrom(s => s.Transmission))
                .ForMember(d => d.NoOfOwner, o => o.MapFrom(s => s.NoOfOwner))
                .ForMember(d => d.IsAccidental, o => o.MapFrom(s => s.IsAccidental))
                .ForMember(d => d.AdjustableStaring, o => o.MapFrom(s => s.AdjustableStaring))
                .ForMember(d => d.AlloyWheels, o => o.MapFrom(s => s.AlloyWheels))
                .ForMember(d => d.AntiTheftSystem, o => o.MapFrom(s => s.AntiTheftSystem))
                .ForMember(d => d.MusicSystem, o => o.MapFrom(s => s.MusicSystem))
                .ForMember(d => d.Aux, o => o.MapFrom(s => s.Aux))
                .ForMember(d => d.Bluetooth, o => o.MapFrom(s => s.Bluetooth))
                .ForMember(d => d.InsuranceType, o => o.MapFrom(s => s.InsuranceType))
                .ForMember(d => d.PowerStaring, o => o.MapFrom(s => s.PowerStaring))
                .ForMember(d => d.PowerWindow, o => o.MapFrom(s => s.PowerWindow))
                .ForMember(d => d.RegistrationState, o => o.MapFrom(s => s.RegistrationState))
                .ForMember(d => d.ServiceHistory, o => o.MapFrom(s => s.ServiceHistory))
                .ForMember(d => d.EnginCapacity, o => o.MapFrom(s => s.EnginCapacity))
                .ForMember(d => d.AirCondition, o => o.MapFrom(s => s.AirCondition))
                .ForMember(d => d.AirBag, o => o.MapFrom(s => s.AirBag))
                .ForMember(d => d.ABS, o => o.MapFrom(s => s.ABS))
                .ForMember(d => d.EBD, o => o.MapFrom(s => s.EBD))
                .ForMember(d => d.BSD, o => o.MapFrom(s => s.BSD))
                .ForMember(d => d.HillHold, o => o.MapFrom(s => s.HillHold))
                .ForMember(d => d.Location, o => o.MapFrom(s =>
                    s.CarLat == null || s.CarLon == null ? null : new GeoLocationDto
                    {
                        Latitude  = s.CarLat.Value,
                        Longitude = s.CarLon.Value
                    }))
                .ForMember(d => d.IsWishlisted, o => o.MapFrom(s => s.IsWishlisted))
                .ForMember(d => d.DistanceKm,   o => o.MapFrom(s => s.DistanceKm))

                // -------- Images (Image1…Image20 → List<string>) --------
                .ForMember(d => d.Images, o => o.MapFrom(s =>
                    new[]
                    {
                    s.Image1, s.Image2, s.Image3, s.Image4, s.Image5,
                    s.Image6, s.Image7, s.Image8, s.Image9, s.Image10,
                    s.Image11, s.Image12, s.Image13, s.Image14, s.Image15,
                    s.Image16, s.Image17, s.Image18, s.Image19, s.Image20
                    }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!)
                    .ToList()
                ));
        }
    }
}
