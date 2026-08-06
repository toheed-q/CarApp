using CommunityToolkit.Maui;

namespace DMF.Pages.Popups;

/// <summary>
/// Shared CommunityToolkit v15 PopupOptions. In v15 the scrim colour, the
/// tap-outside-to-dismiss flag and the default frame moved off the Popup itself
/// and onto the options passed to ShowPopupAsync — these presets keep every
/// popup looking the same as before (dark scrim, no toolkit frame).
/// </summary>
public static class PopupDefaults
{
    // Bottom sheets and dialogs: dim the page behind, dismiss on outside tap,
    // and drop the toolkit's default rounded frame (our content draws its own).
    public static PopupOptions Sheet(bool dismissable = true) => new()
    {
        PageOverlayColor = Color.FromArgb("#B3000000"),
        Shape = null,
        CanBeDismissedByTappingOutsideOfPopup = dismissable,
    };

    // The filter panel used a transparent overlay and could not be dismissed by
    // an outside tap (it has its own Close/Apply buttons).
    public static PopupOptions Filter => new()
    {
        PageOverlayColor = Colors.Transparent,
        Shape = null,
        CanBeDismissedByTappingOutsideOfPopup = false,
    };
}
