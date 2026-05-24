namespace DMF.Models
{
    public class FilterModel
    {
        public string Brand { get; set; } = "All";
        public string Model { get; set; } = "All";
        public int Owners { get; set; }
        public int PriceMore { get; set; }
        public int PriceLess { get; set; }
        public int DrivenMore { get; set; }
        public int DrivenLess { get; set; }
        public int Age { get; set; }
        public int DealersID { get; set; }
        public string SortBy { get; set; } = "ID";
        public string SortDir { get; set; } = "asc";
    }
}
