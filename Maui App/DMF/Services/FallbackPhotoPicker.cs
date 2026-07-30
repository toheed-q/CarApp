using DMF.Services.Interfaces;

namespace DMF.Services
{
    // Non-Android platforms (Windows dev host, etc.): reuse the cross-platform
    // FilePicker. The Android build uses the native gallery photo picker instead.
    public class FallbackPhotoPicker : IPhotoPicker
    {
        public async Task<IReadOnlyList<string>> PickImagesAsync(int max)
        {
            var results = await FilePicker.PickMultipleAsync(new PickOptions
            {
                PickerTitle = "Select Images",
                FileTypes = FilePickerFileType.Images
            });

            return results?.Select(r => r.FullPath).Take(max).ToList()
                   ?? new List<string>();
        }
    }
}
