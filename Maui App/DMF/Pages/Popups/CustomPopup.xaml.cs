using CommunityToolkit.Maui.Views;
using DMF.Enums;

namespace DMF.Pages.Popups;

public partial class CustomPopup : Popup
{
    private readonly Action? _onOk;

    public CustomPopup(PopupModel model, Action? onOk)
    {
        InitializeComponent();

        PopupName.Text = model.PopupName;
        PopupMessage.Text = model.PopupMessage;
        PopupMessage.IsVisible = !string.IsNullOrWhiteSpace(model.PopupMessage);
        OkButton.Text = model.OkText;
        _onOk = onOk;

        ApplyStyle(model.PopupType);
    }

    // Picks the icon + accent colour for the given message type so success,
    // warnings and errors are visually distinct and on-brand.
    private void ApplyStyle(PopupType type)
    {
        var (accent, glyph) = type switch
        {
            PopupType.Success => (Color.FromArgb("#2ECC71"), FluentUI.checkmark_circle_48_regular),
            PopupType.Warning => (Color.FromArgb("#F39C12"), FluentUI.warning_48_regular),
            PopupType.Error   => (Color.FromArgb("#E74C3C"), FluentUI.error_circle_48_regular),
            _                 => (Color.FromArgb("#CA2F49"), FluentUI.info_48_regular),
        };

        IconImage.Source = new FontImageSource
        {
            FontFamily = "FluentUI",
            Glyph = glyph,
            Color = accent,
            Size = 36
        };
        IconBadge.BackgroundColor = accent.WithAlpha(0.16f);
        OkButton.BackgroundColor = accent;
    }

    private void OnOkClicked(object sender, EventArgs e)
    {
        Close();
        _onOk?.Invoke();
    }
}
