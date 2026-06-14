using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics;

namespace DMF.Models
{
    public class AddCarModel
    {
        // -------- Identity (set only when editing an existing car) --------
        public int? ID { get; set; }

        // -------- Dealer --------
        public int? DealersID { get; set; }

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

        // -------- Location --------
        // GPS coordinates (precise point, used for distance sorting).
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Selected city (required). CityId is sent to the API; CityName is kept
        // only for displaying the current selection in the wizard.
        public int? CityId { get; set; }
        public string? CityName { get; set; }
    }

    public partial class ImageItem : ObservableObject
    {
        // For a newly picked photo this is the local file path; for an image
        // loaded when editing a car it holds the existing remote (blob) URL.
        public string FilePath { get; set; } = string.Empty;

        // True when FilePath is an already-uploaded blob URL (edit mode) rather
        // than a freshly picked local file that still needs uploading.
        public bool IsExisting { get; set; }

        // The chosen "primary" photo becomes the listing thumbnail. Exactly one
        // image in a listing should have this set.
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StarBadgeColor))]
        private bool isPrimary;

        // Star badge styling, driven by IsPrimary so the grid updates instantly
        // when the user taps a different photo. Gold = primary, dim = tap to set.
        public Color StarBadgeColor => IsPrimary
            ? Color.FromArgb("#F5A623")
            : Color.FromArgb("#99000000");
    }
}
