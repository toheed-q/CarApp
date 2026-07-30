namespace DMF_Services.DTOs.Cars
{
    public class CreateCarDto
    {
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
        public bool? IsAccidental { get; set; }
        public bool? ServiceHistory { get; set; }
        public bool? AlloyWheels { get; set; }
        public bool? Bluetooth { get; set; }
        public bool? PowerStaring { get; set; }
        public bool? PowerWindow { get; set; }
        public bool? AirBag { get; set; }
        public bool? ABS { get; set; }
        public string? AirCondition { get; set; }

        // Additional listing attributes.
        public string? BodyType { get; set; }
        public bool? IsNegotiable { get; set; }
        public bool? ReverseCamera { get; set; }
        public bool? Sunroof { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Selected city (FK -> CityLocations). Used for exact city filtering,
        // separate from the GPS point above.
        public int? CityId { get; set; }
    }
}
