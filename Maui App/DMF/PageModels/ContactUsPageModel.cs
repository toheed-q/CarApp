using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMF.PageModels
{
    public partial class ContactUsPageModel : ObservableObject
    {
        [RelayCommand] Task Back() => Shell.Current.GoToAsync("..", true);
    }
}
