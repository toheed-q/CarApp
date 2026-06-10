using CommunityToolkit.Mvvm.ComponentModel;

namespace DMF.Models
{
    public partial class CarFilterResult : ObservableObject
    {
        // -------- CarDetail --------
        public int ID { get; set; }
        public int? DealersID { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Varient { get; set; }
        public int? Price { get; set; }
        public string? RegistrationNo { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public int? KMDriven { get; set; }
        public string? Fuel { get; set; }
        public string? Transmission { get; set; }
        public int? NoOfOwner { get; set; }
        public bool? IsAccidental { get; set; }
        public bool? AdjustableStaring { get; set; }
        public bool? AlloyWheels { get; set; }
        public bool? AntiTheftSystem { get; set; }
        public bool? MusicSystem { get; set; }
        public bool? Aux { get; set; }
        public bool? Bluetooth { get; set; }
        public string? InsuranceType { get; set; }
        public bool? PowerStaring { get; set; }
        public bool? PowerWindow { get; set; }
        public string? RegistrationState { get; set; }
        public bool? ServiceHistory { get; set; }
        public int? EnginCapacity { get; set; }
        public string? AirCondition { get; set; }
        public bool? AirBag { get; set; }
        public bool? ABS { get; set; }
        public bool? EBD { get; set; }
        public bool? BSD { get; set; }
        public bool? HillHold { get; set; }
        public string? BodyType { get; set; }
        public bool? IsNegotiable { get; set; }
        public bool? ReverseCamera { get; set; }
        public bool? Sunroof { get; set; }
        public GeoLocationDto? Location { get; set; }
        public DateTime? CreatedDate { get; set; }

        // -------- Dealer info (populated by CarDetailPageModel) --------
        public string? DealerName { get; set; }

        [ObservableProperty]
        private bool isWishlisted;

        // True only when viewing your OWN portfolio — controls Edit/Delete buttons
        [ObservableProperty]
        private bool canManage;

        // -------- Images (API-friendly) --------
        public List<string> Images { get; set; } = new();

        // -------- Computed display properties for CarDetailPage --------
        public string Name => $"{Brand} {Model}".Trim();
        public string Registration_Date => RegistrationDate?.ToString("MMM yyyy") ?? "-";
        public string Km => KMDriven.HasValue ? $"{KMDriven:N0} km" : "-";
        public string Gear => Transmission ?? "-";
        public string Engine_CC => EnginCapacity.HasValue ? $"{EnginCapacity} cc" : "-";
        public string Owner => NoOfOwner.HasValue ? $"{NoOfOwner} owner" : "-";
        public string PriceDisplay => Price.HasValue ? $"₹{Price:N0}" : "-";
        public string PostedOn => CreatedDate.HasValue ? $"Posted on: {CreatedDate:dd MMM yyyy}" : string.Empty;

        // Bool → Yes/No helpers
        private static string YesNo(bool? val) => val == true ? "Yes" : val == false ? "No" : "-";
        public string ABS_Text          => YesNo(ABS);
        public string Accidental_Text   => YesNo(IsAccidental);
        public string AlloyWheels_Text  => YesNo(AlloyWheels);
        public string AntiTheft_Text    => YesNo(AntiTheftSystem);
        public string Aux_Text          => YesNo(Aux);
        public string Bluetooth_Text    => YesNo(Bluetooth);
        public string MusicSystem_Text  => YesNo(MusicSystem);
        public string PowerWindow_Text  => YesNo(PowerWindow);
        public string PowerStaring_Text => YesNo(PowerStaring);
        public string ServiceHistory_Text => YesNo(ServiceHistory);
        public string AirBag_Text       => YesNo(AirBag);
        public string ABS2_Text         => YesNo(ABS);
        public string EBD_Text          => YesNo(EBD);
        public string BSD_Text          => YesNo(BSD);
        public string HillHold_Text     => YesNo(HillHold);

        // Attributes captured on the Add-Car form.
        public string BodyType_Text       => string.IsNullOrWhiteSpace(BodyType) ? "-" : BodyType;
        public string Negotiable_Text     => YesNo(IsNegotiable);
        public string ReverseCamera_Text  => YesNo(ReverseCamera);
        public string Sunroof_Text        => YesNo(Sunroof);
        public string AirConditioning_Text => string.IsNullOrWhiteSpace(AirCondition) ? "-" : AirCondition;
    }
}
