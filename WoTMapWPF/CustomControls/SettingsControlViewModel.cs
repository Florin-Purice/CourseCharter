using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace WoTMapWPF.CustomControls
{
    public partial class SettingsControlViewModel : ViewModelBase
    {
        [ObservableProperty]
        private WriteableBitmap pinBitmap;
        [ObservableProperty]
        private WriteableBitmap pinSelectedBitmap;
        [ObservableProperty]
        private WriteableBitmap dashedPathBitmap;
        [ObservableProperty]
        private Color colorA;
        [ObservableProperty]
        private Color colorB;
        [ObservableProperty]
        private Color colorC;
        [ObservableProperty]
        private bool isPathAutosaveEnabled;
        [ObservableProperty]
        private int lineStippleFactor;
        [ObservableProperty]
        private double lineWidth;
        [ObservableProperty]
        private double pinSize;

        public SettingsControlViewModel()
        {
            InitializeColorsFromSettings();
            int imageHeight = 300;
            BitmapImage bitmapImage = new BitmapImage();
            Uri uri = new Uri("../Res/pinA.png", UriKind.Relative);
            StreamResourceInfo sri = App.GetResourceStream(uri);
            using (Stream stream = sri.Stream)
            {
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.DecodePixelHeight = imageHeight;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                pinBitmap = new WriteableBitmap(bitmapImage);
            }
            uri = new Uri("../Res/pinB.png", UriKind.Relative);
            sri = App.GetResourceStream(uri);
            using (Stream stream = sri.Stream)
            {
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.DecodePixelHeight = imageHeight;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                pinSelectedBitmap = new WriteableBitmap(bitmapImage);
            }
            uri = new Uri("../Res/dashed_path.png", UriKind.Relative);
            sri = App.GetResourceStream(uri);
            using (Stream stream = sri.Stream)
            {
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.DecodePixelHeight = imageHeight;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                dashedPathBitmap = new WriteableBitmap(bitmapImage);
            }
        }

        public event Action? ResetToDefault;

        public void InitializeColorsFromSettings()
        {
            SolidColorBrush? A = Settings.Get<SolidColorBrush>("PinColor");
            ColorA = A != null ? A.Color : default;
            SolidColorBrush? B = Settings.Get<SolidColorBrush>("PinSelectedColor");
            ColorB = B != null ? B.Color : default;
            SolidColorBrush? C = Settings.Get<SolidColorBrush>("DashedPathColor");
            ColorC = C != null ? C.Color : default;
            IsPathAutosaveEnabled = Settings.Get<bool>("IsPathAutosaveEnabled");
            LineStippleFactor = Settings.Get<int>("LineStippleFactor");
            LineWidth = Settings.Get<double>("LineWidth");
            PinSize = Settings.Get<double>("PinSize");
        }

        partial void OnIsPathAutosaveEnabledChanged(bool value) => Settings.Set(nameof(IsPathAutosaveEnabled), value);

        partial void OnLineStippleFactorChanged(int value) => Settings.Set(nameof(LineStippleFactor), value);

        partial void OnLineWidthChanged(double value) => Settings.Set(nameof(LineWidth), value);

        partial void OnPinSizeChanged(double value) => Settings.Set(nameof(PinSize), value);

        partial void OnColorAChanged(Color value) => ChangeColorSetting("PinColor", value, PinBitmap);

        partial void OnColorBChanged(Color value) => ChangeColorSetting("PinSelectedColor", value, PinSelectedBitmap);

        partial void OnColorCChanged(Color value) => ChangeColorSetting("DashedPathColor", value, DashedPathBitmap);

        [RelayCommand]
        public void Reset()
        {
            ConfirmActionWindow caw = new ConfirmActionWindow("Are you sure you want to restore default settings?");
            if (caw.ShowDialog().GetValueOrDefault())
            {
                Settings.RestoreDefault();
                InitializeColorsFromSettings();
                ResetToDefault?.Invoke();
            }
        }

        private void ChangeColorSetting(string settingName, Color newColor, WriteableBitmap writeableBitmap)
        {
            Settings.Set(settingName, new SolidColorBrush(newColor));
            BitmapColorChanger.ChangeColorKeepAlpha(writeableBitmap, newColor);
        }
    }
}
