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
    }
}
