namespace DMF_Services.Models
{
    public class CarImage
    {
        public int Id { get; set; }
        public int CarDetailID { get; set; }

        public string? Image1 { get; set; }
        public string? Image2 { get; set; }
        public string? Image3 { get; set; }
        public string? Image4 { get; set; }
        public string? Image5 { get; set; }
        public string? Image6 { get; set; }
        public string? Image7 { get; set; }
        public string? Image8 { get; set; }
        public string? Image9 { get; set; }
        public string? Image10 { get; set; }
        public string? Image11 { get; set; }
        public string? Image12 { get; set; }
        public string? Image13 { get; set; }
        public string? Image14 { get; set; }
        public string? Image15 { get; set; }
        public string? Image16 { get; set; }
        public string? Image17 { get; set; }
        public string? Image18 { get; set; }
        public string? Image19 { get; set; }
        public string? Image20 { get; set; }

        // Navigation
        public CarDetail CarDetail { get; set; } = null!;
    }
}
