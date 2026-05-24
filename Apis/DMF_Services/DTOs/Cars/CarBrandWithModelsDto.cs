namespace DMF_Services.DTOs.Cars
{
    public class CarBrandWithModelsDto
    {
        public string Brand { get; set; } = string.Empty;
        public List<string> Models { get; set; } = new();
    }
}
