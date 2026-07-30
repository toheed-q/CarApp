using System.Collections;

namespace DMF.Pages;

/// <summary>
/// Full-screen photo viewer: swipe between images, pinch to zoom, drag to pan while
/// zoomed, double-tap to toggle zoom, and a cross/back button to close.
/// </summary>
public partial class ImageViewerPage : ContentPage
{
    private readonly List<string> _images;

    // Zoom/pan state for the image currently being manipulated.
    private double _currentScale = 1;
    private double _startScale = 1;
    private double _xOffset = 0;
    private double _yOffset = 0;

    private const double MinScale = 1;
    private const double MaxScale = 5;

    public ImageViewerPage(IEnumerable<string> images, int startIndex)
    {
        InitializeComponent();

        _images = (images ?? Enumerable.Empty<string>())
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .ToList();

        Carousel.ItemsSource = _images;

        var index = Math.Clamp(startIndex, 0, Math.Max(0, _images.Count - 1));
        Carousel.Position = index;
        UpdateCounter(index);

        // No arrows needed for a single image.
        var many = _images.Count > 1;
        PrevButton.IsVisible = many;
        NextButton.IsVisible = many;
    }

    private async void OnClose(object sender, TappedEventArgs e)
        => await Navigation.PopModalAsync();

    private void OnPrev(object sender, TappedEventArgs e)
    {
        if (Carousel.Position > 0)
            Carousel.Position--;
    }

    private void OnNext(object sender, TappedEventArgs e)
    {
        if (Carousel.Position < _images.Count - 1)
            Carousel.Position++;
    }

    private void OnPositionChanged(object sender, PositionChangedEventArgs e)
    {
        UpdateCounter(e.CurrentPosition);
        // Reset any leftover zoom so each photo opens at fit-scale.
        ResetZoomState();
    }

    private void UpdateCounter(int index)
    {
        var total = _images.Count == 0 ? 1 : _images.Count;
        CounterLabel.Text = $"{index + 1}/{total}";
    }

    // ── Pinch to zoom (anchored at image centre) ─────────────────────────────
    private void OnPinch(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (sender is not Image image) return;

        if (e.Status == GestureStatus.Started)
            _startScale = image.Scale;

        if (e.Status == GestureStatus.Running)
        {
            _currentScale = _startScale + (e.Scale - 1) * _startScale;
            _currentScale = Math.Clamp(_currentScale, MinScale, MaxScale);
            image.Scale = _currentScale;
        }

        if (e.Status == GestureStatus.Completed && image.Scale <= MinScale)
            Reset(image);
    }

    // ── Drag to pan (only while zoomed; otherwise let the carousel swipe) ─────
    private void OnPan(object sender, PanUpdatedEventArgs e)
    {
        if (sender is not Image image) return;
        if (image.Scale <= MinScale) return;

        switch (e.StatusType)
        {
            case GestureStatus.Running:
                image.TranslationX = _xOffset + e.TotalX;
                image.TranslationY = _yOffset + e.TotalY;
                break;
            case GestureStatus.Completed:
                _xOffset = image.TranslationX;
                _yOffset = image.TranslationY;
                break;
        }
    }

    // ── Double-tap toggles between fit and 2.5x ──────────────────────────────
    private void OnDoubleTap(object sender, TappedEventArgs e)
    {
        if (sender is not Image image) return;

        if (image.Scale > MinScale)
        {
            Reset(image);
        }
        else
        {
            _currentScale = 2.5;
            image.Scale = _currentScale;
        }
    }

    private void Reset(Image image)
    {
        image.Scale = MinScale;
        image.TranslationX = 0;
        image.TranslationY = 0;
        ResetZoomState();
    }

    private void ResetZoomState()
    {
        _currentScale = 1;
        _startScale = 1;
        _xOffset = 0;
        _yOffset = 0;
    }
}
