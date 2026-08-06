using CommunityToolkit.Maui.Extensions;
using DMF.Pages.Popups;

namespace DMF.Services
{
    public class PopupService : DMF.Services.Interfaces.IPopupService
    {
        public async Task ShowPopupAsync(PopupModel model, Action? onOk = null)
        {
            var popup = new CustomPopup(model, onOk);
            var page = Application.Current!.Windows[0].Page!;
            await page.ShowPopupAsync(popup, PopupDefaults.Sheet());
        }
    }
}
