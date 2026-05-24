namespace DMF.Pages.Controls;

public partial class AdvancedEntryView : ContentView
{
    public AdvancedEntryView()
    {
        InitializeComponent();
    }

    /* ================= TEXT ================= */
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(AdvancedEntryView),
            string.Empty,
            BindingMode.TwoWay,
            propertyChanged: OnTextPropertyChanged);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (AdvancedEntryView)bindable;
        var value = newValue?.ToString() ?? string.Empty;

        if (control.InputEntry.Text != value)
            control.InputEntry.Text = value;
    }

    /* ================= PREFIX ================= */
    public static readonly BindableProperty PrefixProperty =
        BindableProperty.Create(
            nameof(Prefix),
            typeof(string),
            typeof(AdvancedEntryView),
            string.Empty,
            propertyChanged: (b, o, n) =>
            {
                var control = (AdvancedEntryView)b;
                control.PrefixLabel.Text = n?.ToString();
                control.PrefixLabel.IsVisible = !string.IsNullOrEmpty(n?.ToString());
            });

    public string Prefix
    {
        get => (string)GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    /* ================= PLACEHOLDER ================= */
    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(string),
            typeof(AdvancedEntryView),
            string.Empty,
            propertyChanged: (b, o, n) =>
                ((AdvancedEntryView)b).InputEntry.Placeholder = n?.ToString());

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /* ================= ENTRY KEYBOARD ================= */
    public static readonly BindableProperty EntryKeyboardProperty =
    BindableProperty.Create(
        nameof(EntryKeyboard),
        typeof(Keyboard),
        typeof(AdvancedEntryView),
        Keyboard.Default,
        propertyChanged: (b, o, n) =>
            ((AdvancedEntryView)b).InputEntry.Keyboard = (Keyboard)n);

    public Keyboard EntryKeyboard
    {
        get => (Keyboard)GetValue(EntryKeyboardProperty);
        set => SetValue(EntryKeyboardProperty, value);
    }

    /* ================= DIGITS ONLY ================= */
    public static readonly BindableProperty DigitsOnlyProperty =
    BindableProperty.Create(
        nameof(DigitsOnly),
        typeof(bool),
        typeof(AdvancedEntryView),
        false);

    public bool DigitsOnly
    {
        get => (bool)GetValue(DigitsOnlyProperty);
        set => SetValue(DigitsOnlyProperty, value);
    }

    /* ================= MAX LENGTH ================= */
    public static readonly BindableProperty EntryMaxLengthProperty =
    BindableProperty.Create(
        nameof(EntryMaxLength),
        typeof(int),
        typeof(AdvancedEntryView),
        int.MaxValue,
        propertyChanged: (b, o, n) =>
            ((AdvancedEntryView)b).InputEntry.MaxLength = (int)n);

    public int EntryMaxLength
    {
        get => (int)GetValue(EntryMaxLengthProperty);
        set => SetValue(EntryMaxLengthProperty, value);
    }

    /* ================= RETURN TYPE ================= */
    public static readonly BindableProperty EntryReturnTypeProperty =
        BindableProperty.Create(
            nameof(ReturnType),
            typeof(ReturnType),
            typeof(AdvancedEntryView),
            ReturnType.Default,
            propertyChanged: (b, o, n) =>
            {
                var control = (AdvancedEntryView)b;
                control.InputEntry.ReturnType = (ReturnType)n;
            });

    public ReturnType ReturnType
    {
        get => (ReturnType)GetValue(EntryReturnTypeProperty);
        set => SetValue(EntryReturnTypeProperty, value);
    }

    /* ================= ERROR STATE ================= */
    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(
            nameof(HasError),
            typeof(bool),
            typeof(AdvancedEntryView),
            false,
            propertyChanged: (b, o, n) =>
            {
                var control = (AdvancedEntryView)b;
                var resources = Application.Current?.Resources;
                Color borderColor;

                if ((bool)n)
                {
                    borderColor = Colors.Red;
                }
                else if (resources != null && resources.TryGetValue("DmfGrayE6", out var colorObj) && colorObj is Color color)
                {
                    borderColor = color;
                }
                else
                {
                    borderColor = Colors.Gray; // fallback color
                }

                control.EntryBorder.BorderColor = borderColor;
            });

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    /* ================= EVENTS ================= */
    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var value = e.NewTextValue ?? string.Empty;

        if (DigitsOnly)
            value = new string(value.Where(char.IsDigit).ToArray());

        if (Text != value)
            Text = value;
    }

    private void OnFocused(object sender, FocusEventArgs e)
    {
        Dispatcher.Dispatch(() =>
        {
            InputEntry.Placeholder = string.Empty;
        });
    }
    private void OnUnfocused(object sender, FocusEventArgs e) { if (string.IsNullOrEmpty(Text)) InputEntry.Placeholder = Placeholder; }
}
