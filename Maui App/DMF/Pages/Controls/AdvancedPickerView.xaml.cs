using System.Collections;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Extensions;
using DMF.Pages.Popups;

namespace DMF.Pages.Controls;

/// <summary>
/// A labelled dropdown field. Shows the field name above a tappable box; tapping
/// opens a dark selection sheet (SearchableSelectPopup) so every dropdown matches
/// the app theme instead of the white native Picker dialog.
/// </summary>
public partial class AdvancedPickerView : ContentView
{
    public AdvancedPickerView()
    {
        InitializeComponent();
    }

    /* ================= ITEMS SOURCE ================= */
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(AdvancedPickerView), null);

    public IList ItemsSource
    {
        get => (IList)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /* ================= SELECTED ITEM ================= */
    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(
            nameof(SelectedItem), typeof(object), typeof(AdvancedPickerView),
            null, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((AdvancedPickerView)b).UpdateValueLabel());

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /* ================= TITLE (field name) ================= */
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(
            nameof(Title), typeof(string), typeof(AdvancedPickerView), string.Empty,
            propertyChanged: (b, o, n) => ((AdvancedPickerView)b).FloatingLabel.Text = n?.ToString());

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /* ================= PREFIX (kept for source compatibility; unused) ========= */
    public static readonly BindableProperty PrefixProperty =
        BindableProperty.Create(nameof(Prefix), typeof(string), typeof(AdvancedPickerView), string.Empty);

    public string Prefix
    {
        get => (string)GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    /* ================= ERROR STATE ================= */
    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(
            nameof(HasError), typeof(bool), typeof(AdvancedPickerView), false,
            propertyChanged: (b, o, n) =>
            {
                var control = (AdvancedPickerView)b;
                control.PickerBorder.Stroke = (bool)n
                    ? Colors.Red
                    : (Color)Application.Current!.Resources["DmfGrayE6"];
            });

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    /* ================= BEHAVIOUR ================= */
    private void UpdateValueLabel()
    {
        var text = SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(text))
        {
            ValueLabel.Text = "Select";
            ValueLabel.TextColor = Color.FromArgb("#8A8A8A");
        }
        else
        {
            ValueLabel.Text = text;
            ValueLabel.TextColor = Colors.White;
        }
    }

    private async void OnTapped(object sender, TappedEventArgs e)
    {
        var items = ItemsSource?
            .Cast<object>()
            .Select(x => x?.ToString() ?? string.Empty)
            .Where(s => s.Length > 0)
            .ToList() ?? new List<string>();

        if (items.Count == 0)
            return;

        // Short lists (Yes/No, fuel types…) don't need a search box; keep their
        // natural order. Long lists (brands) get search + A–Z sorting.
        bool showSearch = items.Count > 6;

        var popup = new SearchableSelectPopup(
            items,
            string.IsNullOrWhiteSpace(Title) ? "Select" : Title,
            searchPlaceholder: "Search...",
            showSearch: showSearch,
            sort: showSearch);

        var page = Application.Current?.Windows[0].Page;
        if (page is null) return;

        await page.ShowPopupAsync(popup, PopupDefaults.Sheet());
        var result = popup.SelectedValue;
        if (string.IsNullOrWhiteSpace(result))
            return;

        SelectedItem = result;
    }
}
