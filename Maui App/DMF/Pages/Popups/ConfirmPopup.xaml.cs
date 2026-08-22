using CommunityToolkit.Maui.Views;

namespace DMF.Pages.Popups;

// A dark, on-brand Yes/No confirmation dialog (replaces the white DisplayAlert).
public partial class ConfirmPopup : Popup
{
    /// <summary>True if the user tapped Yes; false if No or dismissed.</summary>
    public bool Confirmed { get; private set; }

    public ConfirmPopup(string title, string message, string yesText = "Yes", string noText = "No")
    {
        InitializeComponent();
        TitleLabel.Text = title;
        MessageLabel.Text = message;
        YesButton.Text = yesText;
        NoButton.Text = noText;
    }

    private async void OnNoClicked(object sender, EventArgs e) => await CloseAsync();

    private async void OnYesClicked(object sender, EventArgs e)
    {
        Confirmed = true;
        await CloseAsync();
    }
}
