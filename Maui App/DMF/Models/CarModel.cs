namespace DMF.Models
{
    public class CarModel
    {
        public int ID { get; set; }
        public int DealersID { get; set; }

        // ================= Basic Info =================

        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Varient { get; set; } = string.Empty;
        public int Price { get; set; }

        // ================= Registration & Engine =================

        public string? RegistrationNo { get; set; } = string.Empty;
        public DateTime? RegistrationDate { get; set; }
        public string? RegistrationState { get; set; }
        public int? EnginCapacity { get; set; }

        // ================= Usage =================
        public string Fuel { get; set; } = string.Empty;
        public int? KMDriven { get; set; }
        public string? Transmission { get; set; }

        // ================= Ownership =================
        public int? NoOfOwner { get; set; }
        public string InsuranceType { get; set; } = string.Empty;

        // ================= UI Helper Properties NEW=================

        public bool? AlloyWheels { get; set; }
        public bool? Bluetooth { get; set; }
        public bool? PowerWindow { get; set; }
        public bool? IsAccidental { get; set; }
        public bool? AdjustableStaring { get; set; }
        public bool? AntiTheftSystem { get; set; }
        public bool? MusicSystem { get; set; }
        public bool? PowerStaring { get; set; }
        public bool? ServiceHistory { get; set; }
        public string? AirCondition { get; set; }
        public bool? AirBag { get; set; }
        public bool? Aux { get; set; }
        public bool? ABS { get; set; }
        public bool? EBD { get; set; }
        public bool? BSD { get; set; }
        public bool? HillHold { get; set; }

        public DateTime? CreatedDate { get; set; }
        public GeoLocationDto? Location { get; set; }
        public ImagesDto? Images { get; set; }
    }
}
