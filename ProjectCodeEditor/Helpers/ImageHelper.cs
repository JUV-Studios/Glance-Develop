using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;

namespace ProjectCodeEditor.Helpers
{
    public static class ImageHelper
    {
        public static string[] ImageFileTypes = new string[] { ".bmp", ".gif", ".ico", ".jpeg", ".jpg", ".png", ".svg", ".tif", ".tiff" };

        public static bool IsImageFile(StorageFile file)
        {
            return ImageFileTypes.Contains(file.FileType.ToLower());
        }

        private static async Task<SoftwareBitmap> GetSoftwareBitmap(StorageFile file)
        {
            SoftwareBitmap bitmap;
            using (var stream = await file.OpenReadAsync())
            {
                var decoder = await BitmapDecoder.CreateAsync(stream);
                bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }

            return bitmap;
        }

        public static async Task<string> ExtractTextFromImage(StorageFile file)
        {
            OcrEngine engine = OcrEngine.TryCreateFromUserProfileLanguages();
            if (engine != null)
            {
                var result = await engine.RecognizeAsync(await GetSoftwareBitmap(file));
                return result.Text;
            }
            else
            {
                return string.Empty;
            }
        }

        public static async Task<StorageFile> LoadImageFileAsync()
        {
            var openPicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            openPicker.FileTypeFilter.Concat(ImageFileTypes);
            var imageFile = await openPicker.PickSingleFileAsync();

            return imageFile;
        }

        public static async Task<BitmapImage> GetBitmapFromImageAsync(StorageFile file)
        {
            if (file == null)
            {
                return null;
            }

            try
            {
                using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    var bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(fileStream);
                    return bitmapImage;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
