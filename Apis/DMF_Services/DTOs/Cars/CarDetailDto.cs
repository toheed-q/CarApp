
using DMF_Services.DTOs.Common;

namespace DMF_Services.DTOs.Cars
{
    public class CarDetailDto
    {
        public int Id { get; set; }
        public int? DealersID { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Varient { get; set; }
        public int? Price { get; set; }
        public string? RegistrationNo { get; set; }
        public DateOnly? RegistrationDate { get; set; }
        public int? KMDriven { get; set; }
        public string? Fuel { get; set; }
        public string? Transmission { get; set; }
        public int? NoOfOwner { get; set; }
        public bool? IsAccidental { get; set; }
        public bool? AdjustableStaring { get; set; }
        public bool? AlloyWheels { get; set; }
        public bool? AntiTheftSystem { get; set; }
        public bool? MusicSystem { get; set; }
        public bool? Aux { get; set; }
        public bool? Bluetooth { get; set; }
        public string? InsuranceType { get; set; }
        public bool? PowerStaring { get; set; }
        public bool? PowerWindow { get; set; }
        public string? RegistrationState { get; set; }
        public bool? ServiceHistory { get; set; }
        public int? EnginCapacity { get; set; }
        public string? AirCondition { get; set; }
        public bool? AirBag { get; set; }
        public bool? ABS { get; set; }
        public bool? EBD { get; set; }
        public bool? BSD { get; set; }
        public bool? HillHold { get; set; }
        public GeoLocationDto? Location { get; set; }
        public DateTime? CreatedDate { get; set; }

        public CarImageDto? Images { get; set; }
    }
}
