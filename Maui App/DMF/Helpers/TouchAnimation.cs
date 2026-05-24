namespace DMF
{
    public static class TouchAnimation
    {
        private static readonly HashSet<VisualElement> _lockedViews = new();

        public static async Task AnimateAsync(
            VisualElement view,
            double pressedScale = 0.9,
            double pressedOpacity = 0.6,
            uint duration = 70)
        {
            if (view == null)
                return;

            // 🚫 Prevent back-to-back taps
            if (_lockedViews.Contains(view))
                return;

            _lockedViews.Add(view);

            try
            {
                view.IsEnabled = false;

                await view.ScaleTo(pressedScale, duration, Easing.CubicOut);
                view.Opacity = pressedOpacity;

                await view.ScaleTo(1, duration, Easing.CubicIn);
                view.Opacity = 1;
            }
            catch
            {
                // Safe ignore
            }
            finally
            {
                view.Opacity = 1;
                view.IsEnabled = true;
                _lockedViews.Remove(view);
            }
        }
    }
}
