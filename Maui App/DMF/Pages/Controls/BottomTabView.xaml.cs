using DMF.Enums;
using System.Windows.Input;

namespace DMF.Pages.Controls;

public partial class BottomTabView : ContentView
{
    public BottomTabView()
    {
        InitializeComponent();
    }
    public static readonly BindableProperty SelectedTabProperty =
        BindableProperty.Create(
            nameof(SelectedTab),
            typeof(TabType),
            typeof(BottomTabView),
            TabType.Home,
            BindingMode.TwoWay);

    public TabType SelectedTab
    {
        get => (TabType)GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }

    public static readonly BindableProperty ChangeTabCommandProperty =
        BindableProperty.Create(
            nameof(ChangeTabCommand),
            typeof(ICommand),
            typeof(BottomTabView));

    public ICommand ChangeTabCommand
    {
        get => (ICommand)GetValue(ChangeTabCommandProperty);
        set => SetValue(ChangeTabCommandProperty, value);
    }
}