namespace DMF.Pages;

public partial class CarDetailPage : ContentPage
{
    public CarDetailPage(CarDetailPageModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}