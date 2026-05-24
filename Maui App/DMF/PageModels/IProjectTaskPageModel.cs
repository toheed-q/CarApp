using CommunityToolkit.Mvvm.Input;
using DMF.Models;

namespace DMF.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}