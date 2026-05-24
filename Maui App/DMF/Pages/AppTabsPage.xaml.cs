using DMF.Pages.Controls;

namespace DMF.Pages;

public partial class AppTabsPage : TabbedPage
{
    private readonly BottomTabView _bottomTabs;

    public AppTabsPage(HomeView home, FavoriteView favorite, AccountView account, BottomTabView bottomTabs)
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        _bottomTabs = bottomTabs;
        _bottomTabs.ChangeTabCommand = new Command<int>(SwitchTab);

        //// ⭐ pages created by DI with ViewModels injected
        //Children.Add(home);
        //Children.Add(favorite);
        //Children.Add(account);

        // Overlay custom tab bar
        this.CurrentPageChanged += (_, __) =>
        {
            _bottomTabs.SelectedTab = (TabType)Children.IndexOf(CurrentPage);
        };
    }

    public void SwitchTab(int index)
    {
        CurrentPage = Children[index];
    }
}