namespace DMF_Services.DTOs.Cities
{
    /// <summary>
    /// Lightweight, read-only city projection for client consumption.
    /// Latitude/Longitude are intentionally NOT exposed — they stay server-side
    /// for distance resolution only.
    /// </summary>
    public class CityDto
    {
        public int    Id       { get; set; }
        public string CityName { get; set; } = string.Empty;
    }
}
