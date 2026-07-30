using CommunityToolkit.Maui.Views;

namespace DMF.Pages.Popups;

/// <summary>
/// A reusable searchable bottom-sheet for picking one string from a list
/// (Brand, Model, …). Closes with the chosen string, or null when dismissed.
/// </summary>
public partial class SearchableSelectPopup : Popup
{
    private readonly List<string> _all;

    public SearchableSelectPopup(IEnumerable<string> items, string title,
        string searchPlaceholder = "Search...", bool showSearch = true, bool sort = true)
    {
        InitializeComponent();

        var cleaned = (items ?? Enumerable.Empty<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct();

        // Small fixed lists (e.g. Yes/No) keep their natural order; long lists sort A–Z.
        _all = (sort ? cleaned.OrderBy(s => s, StringComparer.CurrentCultureIgnoreCase) : cleaned).ToList();

        TitleLabel.Text = title;
        SearchEntry.Placeholder = searchPlaceholder;
        SearchBorder.IsVisible = showSearch;

        BindableLayout.SetItemsSource(ItemsContainer, _all);
        UpdateEmpty(_all.Count == 0);
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        var q = e.NewTextValue?.Trim() ?? string.Empty;

        var filtered = string.IsNullOrWhiteSpace(q)
            ? _all
            : _all.Where(x => x.Contains(q, StringComparison.CurrentCultureIgnoreCase)).ToList();

        BindableLayout.SetItemsSource(ItemsContainer, filtered);
        UpdateEmpty(filtered.Count == 0);
    }

    private void OnRowTapped(object sender, TappedEventArgs e)
    {
        if (sender is VisualElement v && v.BindingContext is string s)
            Close(s);
    }

    private void UpdateEmpty(bool empty)
    {
        EmptyLabel.IsVisible = empty;
        ItemsScroll.IsVisible = !empty;
    }
}
