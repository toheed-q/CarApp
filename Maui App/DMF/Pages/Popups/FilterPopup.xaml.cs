using CommunityToolkit.Maui.Views;

namespace DMF.Pages.Popups;

public partial class FilterPopup : Popup
{
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
    private Border? _activeBrandBorder;

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

    // ── Popular brand tiles ───────────────────────────────────────
    private void OnPopularBrandTapped(object sender, TappedEventArgs e)
    {
        var brand = e.Parameter?.ToString();
        SelectBrand(brand);
        if (_activeBrandBorder != null)
            _activeBrandBorder.Background = new SolidColorBrush(Color.FromArgb("#1E2130"));

        if (sender is Border b)
        {
            b.Background = new SolidColorBrush(Color.FromArgb("#CA2F49"));
            _activeBrandBorder = b;
        }
    }

    // ── All brands list ───────────────────────────────────────────
    private void OnAllBrandsTapped(object sender, TappedEventArgs e)
    {
        AllBrandsList.IsVisible = !AllBrandsList.IsVisible;
    }

    private void OnBrandSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string brand)
            SelectBrand(brand);
    }

    private void SelectBrand(string? brand)
    {
        SelectedBrand = brand;
        AllModelsList.ItemsSource = string.IsNullOrEmpty(brand)
            ? _allModels
            : _allModels; // In real app filter models by brand via API
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
        var maxText = MaxPrice >= 30000000 ? "₹3 Crore+" : FormatPrice(MaxPrice);
        PriceRangeLabel.Text = $"{minText} — {maxText}";
        MinPriceLabel.Text   = minText;
        MaxPriceLabel.Text   = maxText;
    }

    // ── Year ──────────────────────────────────────────────────────
    private Border? _activeYearBorder;

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
            YearFromEntry.Text = YearFrom.ToString();
            YearToEntry.Text   = YearTo.ToString();
        }
    }

    private void OnYearRangeChanged(object sender, TextChangedEventArgs e)
    {
        if (_activeYearBorder != null)
        {
            _activeYearBorder.Background = new SolidColorBrush(Color.FromArgb("#1E2130"));
            _activeYearBorder = null;
        }

        var fromText = YearFromEntry.Text ?? string.Empty;
        var toText   = YearToEntry.Text   ?? string.Empty;

        int.TryParse(fromText, out int from);
        int.TryParse(toText,   out int to);

        bool fromValid = fromText.Length == 4 && from > 0;
        bool toValid   = toText.Length   == 4 && to   > 0;

        if (!fromValid || !toValid)
        {
            YearValidationLabel.Text      = "Please enter a valid 4-digit year.";
            YearValidationLabel.IsVisible = true;
            return;
        }

        YearValidationLabel.IsVisible = false;
        Age      = 0;
        YearFrom = from;
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
            b.Background = new SolidColorBrush(Color.FromArgb("#2A2D3A"));
            b.Stroke = new SolidColorBrush(Color.FromArgb("#6B6B6B"));
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
        SortDatePublished.Background = new SolidColorBrush(Color.FromArgb("#2A2D3A"));
        SortDatePublished.Stroke = new SolidColorBrush(Color.FromArgb("#6B6B6B"));
        BrandSearchEntry.Text = string.Empty;
        MinKmSlider.Value = 0;
        MaxKmSlider.Value = 2000000;
        MinKmLabel.Text = "0";
        MaxKmLabel.Text = "2,000,000+";
        if (_activeKmBorder != null)
            _activeKmBorder.Background = new SolidColorBrush(Color.FromArgb("#1E2130"));
        _activeKmBorder = null;
        YearFromEntry.Text = "2004";
        YearToEntry.Text   = "2025";
        YearFrom = 2004; YearTo = 2025;
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

        // Reset active selections UI
        if (_activeBrandBorder != null)
            _activeBrandBorder.Background = new SolidColorBrush(Color.FromArgb("#1E2130"));
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
        _activeBrandBorder = null;

        // Close with cleared result so HomeViewModel reloads
        Close(new FilterResult { IsCleared = true });
    }

    private void OnApplyClicked(object sender, EventArgs e)
    {
        // Price from sliders
        MinPrice = (int)MinPriceSlider.Value;
        MaxPrice = (int)MaxPriceSlider.Value;
        if (MaxPrice >= 30000000) MaxPrice = 0; // 0 means no upper limit

        MinKm = (int)MinKmSlider.Value;
        MaxKm = (int)MaxKmSlider.Value;
        if (MaxKm >= 2000000) MaxKm = 0;
        int.TryParse(YearFromEntry.Text, out var yf); YearFrom = yf;
        int.TryParse(YearToEntry.Text,   out var yt); YearTo   = yt;
        // Only send Age if user changed from defaults; Age = max car age in years
        bool yearChanged = YearFrom != 2004 || YearTo != DateTime.Now.Year;
        Age = (yearChanged && YearFrom > 0) ? DateTime.Now.Year - YearFrom : 0;

        Close(new FilterResult
        {
            Brand    = SelectedBrand,
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
        });
    }

    private void OnCloseClicked(object sender, EventArgs e) => Close(null);
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
