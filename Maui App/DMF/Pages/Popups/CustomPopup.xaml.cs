using CommunityToolkit.Maui.Views;

namespace DMF.Pages.Popups;

public partial class CustomPopup : Popup
{
    private readonly Action? _onOk;

    public CustomPopup(PopupModel model, Action? onOk)
    {
        InitializeComponent();
        PopupName.Text = model.PopupName;
        PopupMessage.Text = model.PopupMessage;
        _onOk = onOk;
    }

    private void OnOkClicked(object sender, EventArgs e)
    {
        Close();
        _onOk?.Invoke();
    }
}