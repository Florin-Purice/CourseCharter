using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WoTMapWPF.CustomControls
{
    /// <summary>
    /// Interaction logic for LoadMapControl.xaml
    /// </summary>
    public partial class LoadMapControl : UserControl
    {
        public LoadMapControl()
        {
            InitializeComponent();
        }

        private async void PreviewButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (((Button)sender).Content is Image image)
            {
                if (image.Tag == null)
                {
                    image.Tag = new object();
                    MapFileDefinition map = (MapFileDefinition)((Button)sender).DataContext;
                    int imageHeight = (int)image.Height;
                    string? saveLocation = Settings.GetOrDefault<string>("SaveLocation");
                    string imageLocation = $"{saveLocation}\\maps\\{map.ImageMD5}\\map_image{map.ImageExt}";
                    BitmapImage? loadedImage = await Task.Run(() => LoadImage(imageLocation, imageHeight));
                    if (loadedImage != null)
                        image.Source = loadedImage;
                    else
                        image.Tag = null;
                }
            }
        }

        private static BitmapImage? LoadImage(string path, int decodeHeight)
        {
            try
            {
                using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read);
                BitmapImage bitmapImage = new();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = fileStream;
                bitmapImage.DecodePixelHeight = decodeHeight;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
            catch
            {
                return null;
            }
        }
    }
}
