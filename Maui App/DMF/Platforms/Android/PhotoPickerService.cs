using Android.Content;
using Android.Provider;
using DMF.Services.Interfaces;
using AndroidUri = Android.Net.Uri;

namespace DMF
{
    /// <summary>
    /// Android gallery multi-select using the system Photo Picker
    /// (android.provider.action.PICK_IMAGES) — a gallery-style UI with numbered
    /// selection, and no storage permission required. Falls back to
    /// ACTION_GET_CONTENT on devices without the photo picker.
    /// </summary>
    public class PhotoPickerService : IPhotoPicker
    {
        internal const int RequestCode = 0x7A11;
        private static TaskCompletionSource<IReadOnlyList<string>>? _pending;

        public Task<IReadOnlyList<string>> PickImagesAsync(int max)
        {
            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (activity is null)
                return Task.FromResult<IReadOnlyList<string>>(new List<string>());

            // Only one pick at a time.
            _pending?.TrySetResult(new List<string>());
            // RunContinuationsAsynchronously: the result is delivered from inside
            // OnActivityResult (UI thread). Without this flag the awaiting code
            // (adding images, showing popups) runs INLINE inside OnActivityResult
            // before the activity has fully resumed, which silently fails. This
            // posts the continuation back to the dispatcher instead.
            _pending = new TaskCompletionSource<IReadOnlyList<string>>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            Intent intent = new Intent("android.provider.action.PICK_IMAGES");
            intent.SetType("image/*");
            if (max > 1)
                intent.PutExtra("android.provider.extra.PICK_IMAGES_MAX", max);

            // Fallback for devices that don't ship the photo picker.
            if (intent.ResolveActivity(activity.PackageManager!) is null)
            {
                intent = new Intent(Intent.ActionGetContent);
                intent.SetType("image/*");
                intent.AddCategory(Intent.CategoryOpenable);
                intent.PutExtra(Intent.ExtraAllowMultiple, true);
            }

            activity.StartActivityForResult(intent, RequestCode);
            return _pending.Task;
        }

        // Called from MainActivity.OnActivityResult.
        internal static void DeliverResult(Context context, Android.App.Result resultCode, Intent? data)
        {
            var tcs = _pending;
            _pending = null;
            if (tcs is null) return;

            var paths = new List<string>();

            if (resultCode == Android.App.Result.Ok && data is not null)
            {
                if (data.ClipData is not null)
                {
                    for (int i = 0; i < data.ClipData.ItemCount; i++)
                    {
                        var uri = data.ClipData.GetItemAt(i)?.Uri;
                        var p = uri is null ? null : CopyToCache(context, uri);
                        if (p is not null) paths.Add(p);
                    }
                }
                else if (data.Data is not null)
                {
                    var p = CopyToCache(context, data.Data);
                    if (p is not null) paths.Add(p);
                }
            }

            tcs.TrySetResult(paths);
        }

        // The picker returns content:// URIs the rest of the app can't read directly,
        // so copy each into the app cache and hand back a real file path.
        private static string? CopyToCache(Context context, AndroidUri uri)
        {
            try
            {
                var ext = GetExtension(context, uri);
                var name = $"pick_{Guid.NewGuid():N}{ext}";
                var dest = Path.Combine(context.CacheDir!.AbsolutePath, name);

                using var input = context.ContentResolver!.OpenInputStream(uri);
                if (input is null) return null;
                using var output = File.Create(dest);
                input.CopyTo(output);
                return dest;
            }
            catch
            {
                return null;
            }
        }

        private static string GetExtension(Context context, AndroidUri uri)
        {
            var mime = context.ContentResolver?.GetType(uri);
            return mime switch
            {
                "image/png" => ".png",
                "image/webp" => ".webp",
                "image/heic" or "image/heif" => ".heic",
                _ => ".jpg"
            };
        }
    }
}
