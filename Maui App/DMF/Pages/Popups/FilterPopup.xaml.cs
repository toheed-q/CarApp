using CommunityToolkit.Maui.Views;

namespace DMF.Pages.Popups;

public partial class FilterPopup : Popup
{
    /// <summary>Apply/Clear outcome, or null if closed/cancelled.</summary>
    public FilterResult? Outcome { get; private set; }

    // Filter state
    public string? SelectedBrand { get; private set; }
    public string? SelectedModel { get; private set; }
    public int MinPrice { get; private set; }
    public int MaxPrice { get; private set; }
    public int MinKm { get; private set; }
    public int MaxKm { get; private set; }
    public int Age { get; private set; }
    public int YearFrom { get; private set; } = 2004;
    public int YearTo { get; private set; } = 2025;
    public int Owners { get; private set; }
    public string? SelectedFuel { get; private set; }
    public string SortBy { get; private set; } = "price";
    public string SortDir { get; private set; } = "asc";

    private string _activeMenu = "brand";
    private readonly List<string> _allBrands;
    private readonly List<string> _allModels;

    // Multi-brand selection (popular tiles). The user can pick several brands at
    // once (e.g. Tata + Toyota); they are sent to the API comma-separated.
    private readonly HashSet<string> _selectedBrands = new(StringComparer.OrdinalIgnoreCase);

    public FilterPopup(List<string> brands, List<string> models, string initialPanel = "brand")
    {
        InitializeComponent();
        _allBrands = brands;
        _allModels = models;
        AllBrandsList.ItemsSource = brands;
        AllModelsList.ItemsSource = models;

        if (initialPanel != "brand")
        {
            ShowPanel(initialPanel);
            foreach (var b in new[] { BtnBrand, BtnBudget, BtnYear, BtnOwners, BtnKm, BtnFuel, BtnSort })
            {
                b.BackgroundColor = Colors.Transparent;
                b.TextColor = Color.FromArgb("#B4B4B4");
            }
            var activeBtn = initialPanel switch
            {
                "sort"   => BtnSort,
                "budget" => BtnBudget,
                "year"   => BtnYear,
                "owners" => BtnOwners,
                "km"     => BtnKm,
                "fuel"   => BtnFuel,
                _        => BtnBrand
            };
            activeBtn.BackgroundColor = Color.FromArgb("#1E2130");
            activeBtn.TextColor = Colors.White;
        }

        // The XAML paints "Date Published" as the default selected sort, so point the
        // active-sort tracker at it. Without this it stays null, so choosing another
        // option never clears Date Published and two options look selected.
        _activeSortBorder = SortDatePublished;
    }

    // ── Menu navigation ──────────────────────────────────────────
    private void OnMenuClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        var panel = btn.CommandParameter?.ToString() ?? "brand";
        ShowPanel(panel);

        // Reset all menu button styles
        foreach (var b in new[] { BtnBrand, BtnBudget, BtnYear, BtnOwners, BtnKm, BtnFuel, BtnSort })
        {
            b.BackgroundColor = Colors.Transparent;
            b.TextColor = Color.FromArgb("#B4B4B4");
        }
        btn.BackgroundColor = Color.FromArgb("#1E2130");
        btn.TextColor = Colors.White;
    }

    private void ShowPanel(string panel)
    {
        _activeMenu = panel;
        PanelBrand.IsVisible  = panel == "brand";
        PanelBudget.IsVisible = panel == "budget";
        PanelYear.IsVisible   = panel == "year";
        PanelOwners.IsVisible = panel == "owners";
        PanelKm.IsVisible     = panel == "km";
        PanelFuel.IsVisible   = panel == "fuel";
        PanelSort.IsVisible   = panel == "sort";
    }

    // ── Brand search ─────────────────────────────────────────────
    private void OnBrandSearchChanged(object sender, TextChangedEventArgs e)
    {
        var q = e.NewTextValue?.ToLower().Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(q))
        {
            AllBrandsList.ItemsSource = _allBrands;
            AllModelsList.ItemsSource = _allModels;
            AllBrandsList.IsVisible = false;
            AllModelsList.IsVisible = false;
            return;
        }

        var matchedBrands = _allBrands.Where(b => b.ToLower().Contains(q)).ToList();
        var matchedModels = _allModels.Where(m => m.ToLower().Contains(q)).ToList();

        AllBrandsList.ItemsSource = matchedBrands;
        AllModelsList.ItemsSource = matchedModels;
        AllBrandsList.IsVisible = matchedBrands.Count > 0;
        AllModelsList.IsVisible = matchedModels.Count > 0;
    }

    // ── Popular brand tiles (multi-select toggle) ─────────────────
    private void OnPopularBrandTapped(object sender, TappedEventArgs e)
    {
        var brand = e.Parameter?.ToString();
        if (string.IsNullOrWhiteSpace(brand) || sender is not Border b) return;

        if (_selectedBrands.Contains(brand))
        {
            _selectedBrands.Remove(brand);
            b.Background = new SolidColorBrush(Color.FromArgb("#1E2130")); // unselected
        }
        else
        {
            _selectedBrands.Add(brand);
            b.Background = new SolidColorBrush(Color.FromArgb("#CA2F49")); // selected (brand accent)
        }
    }

    // ── All brands list ───────────────────────────────────────────
    private void OnAllBrandsTapped(object sender, TappedEventArgs e)
    {
        AllBrandsList.IsVisible = !AllBrandsList.IsVisible;
    }

    // The list is multi-select; selected items are read at Apply time. No per-change
    // handling needed, but the CollectionView still needs a bound handler.
    private void OnBrandSelected(object sender, SelectionChangedEventArgs e) { }

    // Collects every chosen brand — popular tiles plus the multi-select list —
    // deduplicated, as a comma-separated string (or null when nothing is chosen).
    private string? GetSelectedBrandsCsv()
    {
        var all = new HashSet<string>(_selectedBrands, StringComparer.OrdinalIgnoreCase);
        if (AllBrandsList?.SelectedItems != null)
            foreach (var item in AllBrandsList.SelectedItems)
                if (item is string s && !string.IsNullOrWhiteSpace(s))
                    all.Add(s.Trim());

        return all.Count == 0 ? null : string.Join(",", all);
    }

    // ── All models list ───────────────────────────────────────────
    private void OnAllModelsTapped(object sender, TappedEventArgs e)
    {
        AllModelsList.IsVisible = !AllModelsList.IsVisible;
    }

    private void OnModelSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string model)
            SelectedModel = model;
    }

    private Border? _activeBudgetBorder;

    // ── Budget presets ────────────────────────────────────────────
    private void OnBudgetPresetTapped(object sender, TappedEventArgs e)
    {
        if (_activeBudgetBorder != null)
            _activeBudgetBorder.Background = new SolidColorBrush(Color.FromArgb("#1E2130"));

        if (sender is Border b)
        {
            b.Background = new SolidColorBrush(Color.FromArgb("#CA2F49"));
            _activeBudgetBorder = b;
        }

        var parts = e.Parameter?.ToString()?.Split(',');
        if (parts?.Length == 2)
        {
            int.TryParse(parts[0], out var min);
            int.TryParse(parts[1], out var max);
            MinPrice = min;
            MaxPrice = max;
            MinPriceSlider.Value = min;
            MaxPriceSlider.Value = max == 0 ? 30000000 : max;
            UpdatePriceRangeLabel();
        }
    }

    private void OnMinPriceChanged(object sender, ValueChangedEventArgs e)
    {
        MinPrice = (int)e.NewValue;
        if (MinPrice > MaxPrice && MaxPrice > 0)
        {
            MinPrice = MaxPrice;
            MinPriceSlider.Value = MinPrice;
        }
        UpdatePriceRangeLabel();
    }

    private void OnMaxPriceChanged(object sender, ValueChangedEventArgs e)
    {
        MaxPrice = (int)e.NewValue;
        if (MaxPrice < MinPrice)
        {
            MaxPrice = MinPrice;
            MaxPriceSlider.Value = MaxPrice;
        }
        UpdatePriceRangeLabel();
    }

    private void UpdatePriceRangeLabel()
    {
        string FormatPrice(int p) => p >= 10000000
            ? $"₹{p / 10000000.0:0.#} Cr"
            : p >= 100000
            ? $"₹{p / 100000.0:0.#} Lac"
            : $"₹{p / 1000.0:0.#}K";

        var minText = FormatPrice(MinPrice);
        var maxText = MaxPrice >= 30000000 ? "₹3 Cr+" : FormatPrice(MaxPrice);
        PriceRangeLabel.Text = $"{minText} — {maxText}";
    }

    // ── Year ──────────────────────────────────────────────────────
    private Border? _activeYearBorder;
    // True while a preset is filling the year entries, so the entries' own
    // TextChanged handler doesn't wipe the preset's highlight.
    private bool _settingYearFromPreset;

    private void OnYearPresetTapped(object sender, TappedEventArgs e)
    {
        if (_activeYearBorder != null)
            _activeYearBorder.Background = new SolidColorBrush(Color.FromArgb("#1E2130"));

        if (sender is Border b)
        {
            b.Background = new SolidColorBrush(Color.FromArgb("#CA2F49"));
            _activeYearBorder = b;
        }

        if (int.TryParse(e.Parameter?.ToString(), out var years))
        {
            Age = years == 99 ? 7 : years; // 99 = "7 years and above" sentinel
            var fromYear = DateTime.Now.Year - years;
            YearFrom = years == 99 ? 2004 : fromYear;
            YearTo   = DateTime.Now.Year;

            // Filling the entries fires OnYearRangeChanged, which would clear this
            // preset's highlight — suppress that reset while we set them.
            _settingYearFromPreset = true;
            YearFromEntry.Text = YearFrom.ToString();
            YearToEntry.Text   = YearTo.ToString();
            _settingYearFromPreset = false;
        }
    }

    private void OnYearRangeChanged(object sender, TextChangedEventArgs e)
    {
        // Typing a manual range clears the preset highlight; a preset filling the
        // entries must not.
        if (_settingYearFromPreset) return;

        if (_activeYearBorder != null)
        {
            _activeYearBorder.Background = new SolidColorBrush(Color.FromArgb("#1E2130"));
            _activeYearBorder = null;
        }

        var fromText = YearFromEntry.Text?.Trim() ?? string.Empty;
        var toText   = YearToEntry.Text?.Trim()   ?? string.Empty;

        int.TryParse(fromText, out int from);
        int.TryParse(toText,   out int to);

        // Blank means "no bound" (e.g. no minimum year). Only a non-empty value that
        // isn't a valid 4-digit year is an error.
        bool fromValid = fromText.Length == 0 || (fromText.Length == 4 && from > 0);
        bool toValid   = toText.Length   == 0 || (toText.Length   == 4 && to   > 0);

        if (!fromValid || !toValid)
        {
            YearValidationLabel.Text      = "Please enter a valid 4-digit year.";
            YearValidationLabel.IsVisible = true;
            return;
        }

        YearValidationLabel.IsVisible = false;
        Age      = 0;
        YearFrom = from;   // 0 when blank = no minimum year
        YearTo   = to;
    }

    // ── Owners ────────────────────────────────────────────────────
    private readonly Dictionary<int, (Border box, Label tick)> _ownerCheckboxes = new();

    private void InitOwnerCheckboxes()
    {
        if (_ownerCheckboxes.Count > 0) return;
        _ownerCheckboxes[1] = (ChkOwner1, ChkOwner1Tick);
        _ownerCheckboxes[2] = (ChkOwner2, ChkOwner2Tick);
        _ownerCheckboxes[3] = (ChkOwner3, ChkOwner3Tick);
        _ownerCheckboxes[4] = (ChkOwner4, ChkOwner4Tick);
        _ownerCheckboxes[5] = (ChkOwner5, ChkOwner5Tick);
    }

    private void OnOwnerRowTapped(object sender, TappedEventArgs e)
    {
        InitOwnerCheckboxes();
        if (!int.TryParse(e.Parameter?.ToString(), out var val)) return;

        // Single-select: uncheck all others
        foreach (var (key, (box, tick)) in _ownerCheckboxes)
        {
            bool selected = key == val && Owners != val; // toggle off if already selected
            box.Background = selected ? new SolidColorBrush(Color.FromArgb("#CA2F49")) : new SolidColorBrush(Colors.Transparent);
            box.Stroke = selected ? new SolidColorBrush(Color.FromArgb("#CA2F49")) : new SolidColorBrush(Color.FromArgb("#B4B4B4"));
            tick.IsVisible = selected;
        }
        Owners = Owners == val ? 0 : val;
    }

    private Border? _activeKmBorder;

    // ── KM presets ────────────────────────────────────────────────
    private void OnKmPresetTapped(object sender, TappedEventArgs e)
    {
        if (_activeKmBorder != null)
            _activeKmBorder.Background = new SolidColorBrush(Color.FromArgb("#1E2130"));

        if (sender is Border b)
        {
            b.Background = new SolidColorBrush(Color.FromArgb("#CA2F49"));
            _activeKmBorder = b;
        }

        var parts = e.Parameter?.ToString()?.Split(',');
        if (parts?.Length == 2)
        {
            int.TryParse(parts[0], out var min);
            int.TryParse(parts[1], out var max);
            MinKm = min;
            MaxKm = max;
            MinKmSlider.Value = min;
            MaxKmSlider.Value = max == 0 ? 2000000 : max;
        }
    }

    private void OnMinKmChanged(object sender, ValueChangedEventArgs e)
    {
        MinKm = (int)e.NewValue;
        if (MinKm > MaxKm && MaxKm > 0) { MinKm = MaxKm; MinKmSlider.Value = MinKm; }
        MinKmLabel.Text = MinKm == 0 ? "0" : $"{MinKm:N0}";
    }

    private void OnMaxKmChanged(object sender, ValueChangedEventArgs e)
    {
        MaxKm = (int)e.NewValue;
        if (MaxKm < MinKm) { MaxKm = MinKm; MaxKmSlider.Value = MaxKm; }
        MaxKmLabel.Text = MaxKm >= 2000000 ? "2,000,000+" : $"{MaxKm:N0}";
    }
    // ── Fuel ──────────────────────────────────────────────────────
    private readonly Dictionary<string, (Border box, Label tick)> _fuelCheckboxes = new();

    private void InitFuelCheckboxes()
    {
        if (_fuelCheckboxes.Count > 0) return;
        _fuelCheckboxes["Petrol"]   = (ChkFuelPetrol,   ChkFuelPetrolTick);
        _fuelCheckboxes["Diesel"]   = (ChkFuelDiesel,   ChkFuelDieselTick);
        _fuelCheckboxes["LPG"]      = (ChkFuelLPG,      ChkFuelLPGTick);
        _fuelCheckboxes["CNG"]      = (ChkFuelCNG,      ChkFuelCNGTick);
        _fuelCheckboxes["Electric"] = (ChkFuelElectric, ChkFuelElectricTick);
    }

    private void OnFuelRowTapped(object sender, TappedEventArgs e)
    {
        InitFuelCheckboxes();
        var val = e.Parameter?.ToString() ?? string.Empty;
        foreach (var (key, (box, tick)) in _fuelCheckboxes)
        {
            bool selected = key == val && SelectedFuel != val;
            box.Background = selected ? new SolidColorBrush(Color.FromArgb("#CA2F49")) : new SolidColorBrush(Colors.Transparent);
            box.Stroke = selected ? new SolidColorBrush(Color.FromArgb("#CA2F49")) : new SolidColorBrush(Color.FromArgb("#B4B4B4"));
            tick.IsVisible = selected;
        }
        SelectedFuel = SelectedFuel == val ? null : val;
    }

    // ── Sort ──────────────────────────────────────────────────────
    private Border? _activeSortBorder;

    private void OnSortOptionTapped(object sender, TappedEventArgs e)
    {
        if (_activeSortBorder != null)
        {
            _activeSortBorder.Background = new SolidColorBrush(Colors.Transparent);
            _activeSortBorder.Stroke = new SolidColorBrush(Color.FromArgb("#3A3C48"));
        }
        if (sender is Border b)
        {
            // Same red highlight as every other filter section.
            b.Background = new SolidColorBrush(Color.FromArgb("#CA2F49"));
            b.Stroke = new SolidColorBrush(Color.FromArgb("#CA2F49"));
            _activeSortBorder = b;
        }
        var parts = e.Parameter?.ToString()?.Split(',');
        if (parts?.Length == 2) { SortBy = parts[0]; SortDir = parts[1]; }
    }

    // ── Footer buttons ────────────────────────────────────────────
    private void OnClearClicked(object sender, EventArgs e)
    {
        SelectedBrand = null; SelectedModel = null; SelectedFuel = null;
        MinPrice = 0; MaxPrice = 0; MinKm = 0; MaxKm = 0; Age = 0; Owners = 0;
        SortBy = "date"; SortDir = "desc";
        if (_activeSortBorder != null)
        {
            _activeSortBorder.Background = new SolidColorBrush(Colors.Transparent);
            _activeSortBorder.Stroke = new SolidColorBrush(Color.FromArgb("#3A3C48"));
        }
        _activeSortBorder = SortDatePublished;
        SortDatePublished.Background = new SolidColorBrush(Color.FromArgb("#CA2F49"));
        SortDatePublished.Stroke = new SolidColorBrush(Color.FromArgb("#CA2F49"));
        BrandSearchEntry.Text = string.Empty;
        MinKmSlider.Value = 0;
        MaxKmSlider.Value = 2000000;
        MinKmLabel.Text = "0";
        MaxKmLabel.Text = "2,000,000+";
        if (_activeKmBorder != null)
            _activeKmBorder.Background = new SolidColorBrush(Color.FromArgb("#1E2130"));
        _activeKmBorder = null;
        YearFromEntry.Text = string.Empty;   // no minimum year by default
        YearToEntry.Text   = "2025";
        YearFrom = 0; YearTo = 2025;
        YearValidationLabel.IsVisible = false;
        if (_activeYearBorder != null)
            _activeYearBorder.Background = new SolidColorBrush(Color.FromArgb("#1E2130"));
        _activeYearBorder = null;
        MinPriceSlider.Value = 10000;
        MaxPriceSlider.Value = 30000000;
        UpdatePriceRangeLabel();
        if (_activeBudgetBorder != null)
            _activeBudgetBorder.Background = new SolidColorBrush(Color.FromArgb("#1E2130"));
        _activeBudgetBorder = null;

        // Reset brand selection. The popup closes right after Clear, so the tile
        // colours reset naturally on next open — only the data needs clearing here.
        _selectedBrands.Clear();
        AllBrandsList.SelectedItems?.Clear();
        InitFuelCheckboxes();
        foreach (var (_, (box, tick)) in _fuelCheckboxes)
        {
            box.Background = new SolidColorBrush(Colors.Transparent);
            box.Stroke = new SolidColorBrush(Color.FromArgb("#B4B4B4"));
            tick.IsVisible = false;
        }
        InitOwnerCheckboxes();
        foreach (var (_, (box, tick)) in _ownerCheckboxes)
        {
            box.Background = new SolidColorBrush(Colors.Transparent);
            box.Stroke = new SolidColorBrush(Color.FromArgb("#B4B4B4"));
            tick.IsVisible = false;
        }

        // Close with cleared result so HomeViewModel reloads
        Outcome = new FilterResult { IsCleared = true };
        _ = CloseAsync();
    }

    private async void OnApplyClicked(object sender, EventArgs e)
    {
        // Price from sliders
        MinPrice = (int)MinPriceSlider.Value;
        MaxPrice = (int)MaxPriceSlider.Value;
        // Safety net: never send a reversed range to the search (that produced the
        // "min > max -> Result Not Found" bug). Swap if the user crossed them.
        if (MinPrice > MaxPrice)
            (MinPrice, MaxPrice) = (MaxPrice, MinPrice);
        if (MaxPrice >= 30000000) MaxPrice = 0; // 0 means no upper limit

        MinKm = (int)MinKmSlider.Value;
        MaxKm = (int)MaxKmSlider.Value;
        if (MaxKm >= 2000000) MaxKm = 0;
        int.TryParse(YearFromEntry.Text, out var yf); YearFrom = yf;
        int.TryParse(YearToEntry.Text,   out var yt); YearTo   = yt;
        // Only send Age if user changed from defaults; Age = max car age in years
        bool yearChanged = YearFrom != 2004 || YearTo != DateTime.Now.Year;
        Age = (yearChanged && YearFrom > 0) ? DateTime.Now.Year - YearFrom : 0;

        Outcome = new FilterResult
        {
            Brand    = GetSelectedBrandsCsv(),
            Model    = SelectedModel,
            Fuel     = SelectedFuel,
            MinPrice = MinPrice,
            MaxPrice = MaxPrice,
            MinKm    = MinKm,
            MaxKm    = MaxKm,
            Age      = Age,
            Owners   = Owners,
            SortBy   = SortBy,
            SortDir  = SortDir
        };
        await CloseAsync();
    }

    private async void OnCloseClicked(object sender, EventArgs e) => await CloseAsync();
}

public class FilterResult
{
    public bool    IsCleared { get; set; }
    public string? Brand    { get; set; }
    public string? Model    { get; set; }
    public string? Fuel     { get; set; }
    public int     MinPrice { get; set; }
    public int     MaxPrice { get; set; }
    public int     MinKm    { get; set; }
    public int     MaxKm    { get; set; }
    public int     Age      { get; set; }
    public int     YearFrom { get; set; }
    public int     YearTo   { get; set; }
    public int     Owners   { get; set; }
    public string  SortBy   { get; set; } = "price";
    public string  SortDir  { get; set; } = "asc";
}
