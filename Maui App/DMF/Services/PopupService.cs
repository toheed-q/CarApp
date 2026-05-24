using CommunityToolkit.Maui.Views;
using DMF.Pages.Popups;

namespace DMF.Services
{
    public class PopupService : IPopupService
    {
        public async Task ShowPopupAsync(PopupModel model, Action? onOk = null)
        {
            var popup = new CustomPopup(model, onOk);
            _ = await Application.Current.MainPage.ShowPopupAsync(popup);
        }
    }
}
