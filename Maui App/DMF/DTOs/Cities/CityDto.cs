namespace DMF.DTOs.Cities
{
    /// <summary>
    /// Lightweight, read-only city model returned by GET /api/1.0/cities.
    /// Mirrors the server contract — Latitude/Longitude are intentionally NOT
    /// present so the UI layer can never depend on raw coordinates.
    /// </summary>
    public class CityDto
    {
        public int    Id       { get; set; }
        public string CityName { get; set; } = string.Empty;
    }
}
