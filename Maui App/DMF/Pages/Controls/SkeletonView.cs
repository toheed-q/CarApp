namespace DMF.Pages.Controls
{
    public class SkeletonView : BoxView
    {
        public SkeletonView()
        {
            StartShimmer();
        }

        private void StartShimmer()
        {
            this.Dispatcher.StartTimer(TimeSpan.FromSeconds(1.5), () =>
            {
                _ = AnimateShimmer();
                return true; // keep repeating
            });
        }

        private async Task AnimateShimmer()
        {
            await this.FadeTo(0.5, 750, Easing.CubicInOut);
            await this.FadeTo(1, 750, Easing.CubicInOut);
        }
    }
}
