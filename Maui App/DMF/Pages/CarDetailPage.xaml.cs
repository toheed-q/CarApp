namespace DMF.Pages;

public partial class CarDetailPage : ContentPage
{
    public CarDetailPage(CarDetailPageModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    // Tap a photo -> open the full-screen zoomable gallery at that image.
    private async void OnImageTapped(object sender, TappedEventArgs e)
    {
        if (BindingContext is not DMF.PageModels.CarDetailPageModel vm)
            return;

        var images = vm.CarDetail?.Images;
        if (images is null || images.Count == 0)
            return;

        await Navigation.PushModalAsync(new ImageViewerPage(images, vm.CurrentImageIndex));
    }
}
