using DMF.Enums;

namespace DMF.Models
{
    public class PopupModel
    {
        public string? PopupName { get; set; }
        public string? PopupMessage { get; set; }

        // Drives the icon + accent colour of the styled popup.
        public PopupType PopupType { get; set; } = PopupType.Info;

        // Text on the primary button (defaults to "OK").
        public string OkText { get; set; } = "OK";
    }
}
