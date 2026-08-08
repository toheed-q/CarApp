using CommunityToolkit.Maui;
using Microsoft.Maui.Controls.Shapes;

namespace DMF.Pages.Popups;

/// <summary>
/// Shared CommunityToolkit v15 PopupOptions. In v15 the scrim colour, the
/// tap-outside-to-dismiss flag and the popup frame moved off the Popup itself
/// and onto the options passed to ShowPopupAsync. The toolkit draws a default
/// WHITE rounded frame around the content — setting Shape = null does NOT remove
/// it (the toolkit falls back to its default), so we supply an explicit fully
/// transparent frame (no fill, no stroke, no shadow). Our own content Borders
/// draw the dark card, so the popup reads as one clean dark sheet on any theme.
/// </summary>
public static class PopupDefaults
{
    // A completely invisible popup frame so only our content is visible.
    static RoundRectangle NoFrame() => new()
    {
        CornerRadius = new CornerRadius(0),
        Fill = new SolidColorBrush(Colors.Transparent),
        Stroke = new SolidColorBrush(Colors.Transparent),
        StrokeThickness = 0,
    };

    // Bottom sheets and dialogs: dim the page behind, dismiss on outside tap.
    public static PopupOptions Sheet(bool dismissable = true) => new()
    {
        PageOverlayColor = Color.FromArgb("#B3000000"),
        Shape = NoFrame(),
        Shadow = null,
        CanBeDismissedByTappingOutsideOfPopup = dismissable,
    };

    // The filter panel used a transparent overlay and could not be dismissed by
    // an outside tap (it has its own Close/Apply buttons).
    public static PopupOptions Filter => new()
    {
        PageOverlayColor = Colors.Transparent,
        Shape = NoFrame(),
        Shadow = null,
        CanBeDismissedByTappingOutsideOfPopup = false,
    };
}
