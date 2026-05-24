namespace DMF.Services.Interfaces
{
    public interface IPopupService
    {
        Task ShowPopupAsync(PopupModel model, Action? onOk = null);
    }
}
