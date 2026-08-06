using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMF.PageModels
{
    public partial class ContactUsPageModel : ObservableObject
    {
        [RelayCommand] Task Back() => Shell.Current.GoToAsync("..", true);

        // Opens the phone dialler pre-filled with the support number (as-is).
        [RelayCommand]
        async Task Call()
        {
            try { await Launcher.Default.OpenAsync("tel:8826064422"); }
            catch { /* no dialer available */ }
        }
    }
}
