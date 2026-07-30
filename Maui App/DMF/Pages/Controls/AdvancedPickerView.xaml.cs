using System.Collections;

namespace DMF.Pages.Controls;

public partial class AdvancedPickerView : ContentView
{
    public AdvancedPickerView()
    {
        InitializeComponent();
    }

    /* ================= ITEMS SOURCE ================= */

    public static readonly BindableProperty ItemsSourceProperty =
    BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IList),
        typeof(AdvancedPickerView),
        null,
        propertyChanged: (b, o, n) =>
        {
            var control = (AdvancedPickerView)b;
            control.InputPicker.ItemsSource = (IList)n;
        });

    public IList ItemsSource
    {
        get => (IList)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /* ================= SELECTED ITEM ================= */

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(
            nameof(SelectedItem),
            typeof(object),
            typeof(AdvancedPickerView),
            null,
            BindingMode.TwoWay,
            propertyChanged: (b, o, n) =>
            {
                var control = (AdvancedPickerView)b;

                // Treat an empty/blank value as "nothing selected" so the Picker shows
                // its Title as the placeholder instead of selecting a non-existent item.
                var value = n as string;
                var normalized = string.IsNullOrWhiteSpace(value) ? null : n;

                if (control.InputPicker.SelectedItem != normalized)
                    control.InputPicker.SelectedItem = normalized;

                // NOTE: the "Title  - " prefix was removed. It doubled the label
                // (e.g. "Transmission - Transmission") because the Picker already shows
                // the Title as its placeholder. Set Prefix explicitly if you ever need one.
            });

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /* ================= TITLE ================= */

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(AdvancedPickerView),
            string.Empty,
            propertyChanged: (b, o, n) =>
                ((AdvancedPickerView)b).InputPicker.Title = n?.ToString());

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /* ================= PREFIX ================= */

    public static readonly BindableProperty PrefixProperty =
        BindableProperty.Create(
            nameof(Prefix),
            typeof(string),
            typeof(AdvancedPickerView),
            string.Empty,
            propertyChanged: (b, o, n) =>
            {
                var control = (AdvancedPickerView)b;
                control.PrefixLabel.Text = n?.ToString();
                //control.PrefixLabel.IsVisible = !string.IsNullOrEmpty(n?.ToString());
            });

    public string Prefix
    {
        get => (string)GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    /* ================= ERROR STATE ================= */

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(
            nameof(HasError),
            typeof(bool),
            typeof(AdvancedPickerView),
            false,
            propertyChanged: (b, o, n) =>
            {
                var control = (AdvancedPickerView)b;

                if ((bool)n)
                    control.PickerBorder.Stroke = Colors.Red;
                else
                    control.PickerBorder.Stroke = (Color)Application.Current.Resources["DmfGrayE6"];
            });

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    /* ================= EVENTS ================= */

    private void OnSelectedIndexChanged(object sender, EventArgs e)
    {
        if (SelectedItem != InputPicker.SelectedItem)
            SelectedItem = InputPicker.SelectedItem;
    }

    private void OnDropdownTapped(object sender, EventArgs e)
    {
        InputPicker.Focus();
    }
}