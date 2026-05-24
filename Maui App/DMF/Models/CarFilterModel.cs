namespace DMF.Models
{
    public class CarFilterModel
    {
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Search { get; set; }
        public string? Fuel { get; set; }
        public string? Transmission { get; set; }

        public int Owners { get; set; } = 0;

        public int PriceMore { get; set; } = 0;
        public int PriceLess { get; set; } = 0;

        public int DrivenMore { get; set; } = 0;
        public int DrivenLess { get; set; } = 0;

        public int Age { get; set; } = 0;

        public int UserDetailID { get; set; } = 0;
        public int DealersID { get; set; } = 0;

        public int IsActive { get; set; } = 1;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string SortBy { get; set; } = "price";
        public string SortDir { get; set; } = "asc";
    }
}
