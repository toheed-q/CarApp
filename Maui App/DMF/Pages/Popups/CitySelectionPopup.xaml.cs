using CommunityToolkit.Maui.Views;
using DMF.DTOs.Cities;
using DMF.Services.Interfaces;

namespace DMF.Pages.Popups;

public partial class CitySelectionPopup : Popup
{
    private readonly ICityService _cityService;
    private List<CityDto> _allCities = new();

    public CitySelectionPopup(ICityService cityService, bool allowAllLocations = true)
    {
        InitializeComponent();
        _cityService = cityService;

        // In the search bar "All Locations" clears the filter; on the upload
        // form a city is required, so the reset row is hidden there.
        AllLocationsRow.IsVisible = allowAllLocations;

        // Load lazily once the popup is actually on screen so the spinner
        // (defined as the default visible state in XAML) shows during the fetch.
        Opened += async (_, _) => await LoadCitiesAsync();
    }

    // ── Fetch active cities (loading / empty / error handled) ─────────────
    private async Task LoadCitiesAsync()
    {
        ShowLoading();

        try
        {
            var response = await _cityService.GetActiveCitiesAsync();

            if (response == null || !response.Success)
            {
                ShowMessage("Couldn't load locations. Please try again.");
                return;
            }

            _allCities = response.Data ?? new List<CityDto>();

            if (_allCities.Count == 0)
            {
                ShowMessage("No locations available");
                return;
            }

            BindableLayout.SetItemsSource(CitiesContainer, _allCities);
            ShowList();
        }
        catch (Exception)
        {
            // API failure / slow-or-no network — never crash the popup.
            ShowMessage("Couldn't load locations. Please check your connection.");
        }
    }

    // ── In-memory search inside the loaded list ───────────────────────────
    private void OnCitySearchChanged(object sender, TextChangedEventArgs e)
    {
        if (_allCities.Count == 0) return;

        var q = e.NewTextValue?.Trim().ToLower() ?? string.Empty;

        var filtered = string.IsNullOrWhiteSpace(q)
            ? _allCities
            : _allCities.Where(c => c.CityName.ToLower().Contains(q)).ToList();

        BindableLayout.SetItemsSource(CitiesContainer, filtered);

        bool any = filtered.Count > 0;
        CitiesScroll.IsVisible = any;
        EmptyLabel.IsVisible = !any;
        if (!any) EmptyLabel.Text = "No cities found";
    }

    // ── Selection -> return CityDto to caller ─────────────────────────────
    private void OnCityRowTapped(object sender, TappedEventArgs e)
    {
        if (sender is VisualElement v && v.BindingContext is CityDto city)
            Close(new CitySelectionResult { City = city });
    }

    // ── "All Locations" -> explicit clear ─────────────────────────────────
    private void OnAllLocationsTapped(object sender, TappedEventArgs e)
        => Close(new CitySelectionResult { IsCleared = true });

    // ── State helpers ─────────────────────────────────────────────────────
    private void ShowLoading()
    {
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
        EmptyLabel.IsVisible = false;
        CitiesScroll.IsVisible = false;
    }

    private void ShowList()
    {
        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
        EmptyLabel.IsVisible = false;
        CitiesScroll.IsVisible = true;
    }

    private void ShowMessage(string message)
    {
        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
        CitiesScroll.IsVisible = false;
        EmptyLabel.Text = message;
        EmptyLabel.IsVisible = true;
    }
}

/// <summary>
/// Result returned from <see cref="CitySelectionPopup"/>.
/// null  = cancelled (retain previous selection),
/// IsCleared = reset to "All Locations",
/// City  = a city was chosen.
/// </summary>
public class CitySelectionResult
{
    public bool     IsCleared { get; set; }
    public CityDto? City      { get; set; }
}
