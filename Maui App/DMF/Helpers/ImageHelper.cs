using SkiaSharp;

namespace DMF.Helpers
{
    public class ImageHelper
    {
        public static Stream CompressImage(string filePath, int quality = 75)
        {
            using var input = File.OpenRead(filePath);
            using var bitmap = SKBitmap.Decode(input);

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);

            return new MemoryStream(data.ToArray());
        }

        /// <summary>
        /// Center-crops an image to a square and resizes it to <paramref name="size"/>px.
        /// Used for profile avatars so they always render cleanly inside a circle.
        /// </summary>
        public static Stream CropToSquare(string filePath, int size = 512, int quality = 85)
        {
            using var input = File.OpenRead(filePath);
            using var source = SKBitmap.Decode(input);

            // Largest centered square that fits inside the source image.
            var edge = Math.Min(source.Width, source.Height);
            var left = (source.Width - edge) / 2;
            var top = (source.Height - edge) / 2;
            var cropRect = new SKRectI(left, top, left + edge, top + edge);

            using var square = new SKBitmap(edge, edge);
            source.ExtractSubset(square, cropRect);

            // Scale the square to the target avatar size.
            var info = new SKImageInfo(size, size);
            var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);
            using var resized = square.Resize(info, sampling) ?? square;

            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);

            return new MemoryStream(data.ToArray());
        }
    }
}
