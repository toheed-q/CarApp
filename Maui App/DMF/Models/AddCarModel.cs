namespace DMF.Models
{
    public class AddCarModel
    {
        // -------- Basic Details --------
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int? YearOfManufacture { get; set; }
        public string RegistrationNo { get; set; } = string.Empty;
        public DateTime? PurchaseDate { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public string Transmission { get; set; } = string.Empty;
        public string BodyType { get; set; } = string.Empty;

        // -------- Vehicle Condition --------
        public int? OdometerReading { get; set; }
        public bool? AccidentHistory { get; set; }
        public bool? ServiceHistory { get; set; }

        // -------- Pricing --------
        public int? Price { get; set; }
        public bool? IsNegotiable { get; set; }

        // -------- Features --------
        public bool? AirCondition { get; set; }
        public bool? PowerSteering { get; set; }
        public bool? PowerWindow { get; set; }
        public bool? ABS { get; set; }
        public bool? Airbags { get; set; }
        public bool? Bluetooth { get; set; }
        public bool? ReverseCamera { get; set; }
        public bool? AlloyWheels { get; set; }
        public bool? Sunroof { get; set; }

        // -------- Images --------
        public List<string> Images { get; set; } = new();
    }

    public class ImageItem
    {
        public string FilePath { get; set; } = string.Empty;
    }
}
