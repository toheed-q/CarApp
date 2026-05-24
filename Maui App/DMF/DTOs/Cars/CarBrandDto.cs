namespace DMF.DTOs.Cars
{
    public class CarBrandDto
    {
        public string Brand { get; set; } = string.Empty;
    }

    public class CarModelDto
    {
        public string Model { get; set; } = string.Empty;
    }

    public class CarBrandWithModelsDto
    {
        public string Brand { get; set; } = string.Empty;
        public List<string> Models { get; set; } = new();
    }
}
