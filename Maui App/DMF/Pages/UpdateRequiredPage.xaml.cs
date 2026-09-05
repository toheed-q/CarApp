using DMF.Helpers;

namespace DMF.Pages;

public partial class UpdateRequiredPage : ContentPage
{
    public UpdateRequiredPage()
    {
        InitializeComponent();

        if (!string.IsNullOrWhiteSpace(AppUpdateHelper.Message))
            MessageLabel.Text = AppUpdateHelper.Message;

        // "Later" only for optional updates; a forced update has no way out but to update.
        LaterButton.IsVisible = !AppUpdateHelper.IsForced;
    }

    // Block the hardware back button so a forced update can't be bypassed.
    protected override bool OnBackButtonPressed() => true;

    private async void OnUpdateClicked(object sender, EventArgs e)
        => await AppUpdateHelper.OpenStoreAsync();

    private async void OnLaterClicked(object sender, EventArgs e)
        => await AppNavigation.GoToStartDestinationAsync();
}
