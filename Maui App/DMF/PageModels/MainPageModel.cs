using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMF.PageModels;

public partial class MainPageModel : ObservableObject
{
    private readonly HomeView _homeView;
    private readonly FavoriteView _favoriteView;
    private readonly AccountView _accountView;
    private readonly HomeViewModel _homeViewModel;
    private readonly AccountViewModel _accountViewModel;
    private readonly FavoriteViewModel _favoriteViewModel;

    [ObservableProperty]
    private View currentView = null!;

    [ObservableProperty]
    private TabType selectedTab;

    // Set by the login / OTP flow so the next time MainPage appears it lands on the
    // Home tab — the tab bar is a reused singleton that would otherwise keep whatever
    // tab (e.g. Account) was open before the user logged out.
    public static bool ForceHomeOnAppear;

    [ObservableProperty]
    private string _bgImage = "get_started_bg";

    public MainPageModel(HomeViewModel _homeViewModel, FavoriteViewModel _favoriteViewModel, AccountViewModel _accountViewModel)
    {
        _homeView = new HomeView(_homeViewModel);
        _favoriteView = new FavoriteView(_favoriteViewModel);
        _accountView = new AccountView(_accountViewModel);
        this._homeViewModel = _homeViewModel;
        this._accountViewModel = _accountViewModel;
        this._favoriteViewModel = _favoriteViewModel;
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

        // These views are cached in this singleton, so their one-time
        // Loaded/OnHandlerChanged load won't re-run when the tab is revisited.
        // Refresh from storage each time the tab is shown so changes elsewhere
        // (re-login, newly wishlisted cars) are reflected.
        if (value == TabType.Home)
            _ = _homeViewModel.EnsureLoadedForCurrentUserAsync();
        else if (value == TabType.Account)
            _ = _accountViewModel.LoadUserAsync();
        else if (value == TabType.Favorite)
            _favoriteViewModel.Initialize();

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

    // Same idea for the cached Home view: re-check the signed-in user so wishlist
    // hearts reflect the current account, not the previous one. Reloads only if the
    // user changed.
    public Task RefreshHomeAsync() => _homeViewModel.EnsureLoadedForCurrentUserAsync();
}