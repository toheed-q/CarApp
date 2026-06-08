using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMF.PageModels;

public partial class MainPageModel : ObservableObject
{
    private readonly HomeView _homeView;
    private readonly FavoriteView _favoriteView;
    private readonly AccountView _accountView;
    private readonly AccountViewModel _accountViewModel;

    [ObservableProperty]
    private View currentView = null!;

    [ObservableProperty]
    private TabType selectedTab;

    [ObservableProperty]
    private string _bgImage = "get_started_bg";

    public MainPageModel(HomeViewModel _homeViewModel, FavoriteViewModel _favoriteViewModel, AccountViewModel _accountViewModel)
    {
        _homeView = new HomeView(_homeViewModel);
        _favoriteView = new FavoriteView(_favoriteViewModel);
        _accountView = new AccountView(_accountViewModel);
        this._accountViewModel = _accountViewModel;
    }

    public void Initialize()
    {
        SelectedTab = TabType.Home;
        CurrentView = _homeView;
    }

    partial void OnSelectedTabChanged(TabType value)
    {
        CurrentView = value switch
        {
            TabType.Home => _homeView,
            TabType.Favorite => _favoriteView,
            TabType.Account => _accountView,
            _ => _homeView
        };

        // The Account view is cached in this singleton, so its one-time
        // OnHandlerChanged load won't re-run after a re-login. Refresh the
        // name from storage every time the Account tab is shown.
        if (value == TabType.Account)
            _ = _accountViewModel.LoadUserAsync();

        BgImage = value switch
        {
            TabType.Home => "get_started_bg",
            TabType.Favorite => "favorite_bg",
            TabType.Account => "account_bg",
            _ => "get_started_bg"
        };
    }

    [RelayCommand]
    private void ChangeTab(TabType tab)
    {
        SelectedTab = tab;
    }

    // Called when MainPage re-appears (e.g. after a re-login) so the cached
    // Account view picks up the newly logged-in user from storage.
    public Task RefreshAccountAsync() => _accountViewModel.LoadUserAsync();
}