namespace DMF.Pages;

public partial class OTPVerificationPage : ContentPage
{
    public OTPVerificationPage(OTPVerificationPageModel pageModel)
    {
        try
        {
            InitializeComponent();
            this.BindingContext = pageModel;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private void OnOtpTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry)
            return;

        // FORWARD FOCUS
        if (!string.IsNullOrEmpty(e.NewTextValue) && e.NewTextValue.Length == 1)
        {
            if (entry == OtpEntry1)
                OtpEntry2.Focus();
            else if (entry == OtpEntry2)
                OtpEntry3.Focus();
            else if (entry == OtpEntry3)
                OtpEntry4.Focus();
        }

        // BACKWARD FOCUS (on delete)
        if (string.IsNullOrEmpty(e.NewTextValue))
        {
            if (entry == OtpEntry4)
                OtpEntry3.Focus();
            else if (entry == OtpEntry3)
                OtpEntry2.Focus();
            else if (entry == OtpEntry2)
                OtpEntry1.Focus();
        }
    }
}