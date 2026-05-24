namespace DMF.Pages;

public partial class AddCarStep1Page : ContentPage
{
    public AddCarStep1Page(AddCarViewModel vm)
    {
        try
        {
            InitializeComponent();

            this.BindingContext = vm;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}