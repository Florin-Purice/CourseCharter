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
        public SettingsControlViewModel()
        {
            int imageHeight = 300;
            BitmapImage bitmapImage = new();
            Uri uri = new("../Res/pinA.png", UriKind.Relative);
            StreamResourceInfo sri = App.GetResourceStream(uri);
            using (Stream stream = sri.Stream)
            {
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.DecodePixelHeight = imageHeight;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                PinBitmap = new WriteableBitmap(bitmapImage);
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
                PinSelectedBitmap = new WriteableBitmap(bitmapImage);
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
                DashedPathBitmap = new WriteableBitmap(bitmapImage);
            }
            InitializeColorsFromSettings();
        }

        public event Action? ResetToDefault;

        [ObservableProperty]
        public partial WriteableBitmap PinBitmap { get; set; }
        [ObservableProperty]
        public partial WriteableBitmap PinSelectedBitmap { get; set; }
        [ObservableProperty]
        public partial WriteableBitmap DashedPathBitmap { get; set; }
        [ObservableProperty]
        public partial Color ColorA { get; set; }
        [ObservableProperty]
        public partial Color ColorB { get; set; }
        [ObservableProperty]
        public partial Color ColorC { get; set; }
        [ObservableProperty]
        public partial bool IsPathAutosaveEnabled { get; set; }
        [ObservableProperty]
        public partial int LineStippleFactor { get; set; }
        [ObservableProperty]
        public partial double LineWidth { get; set; }
        [ObservableProperty]
        public partial double PinSize { get; set; }

        public void InitializeColorsFromSettings()
        {
            SolidColorBrush? A = Settings.GetOrDefault<SolidColorBrush>("PinColor");
            ColorA = A != null ? A.Color : default;
            SolidColorBrush? B = Settings.GetOrDefault<SolidColorBrush>("PinSelectedColor");
            ColorB = B != null ? B.Color : default;
            SolidColorBrush? C = Settings.GetOrDefault<SolidColorBrush>("DashedPathColor");
            ColorC = C != null ? C.Color : default;
            IsPathAutosaveEnabled = Settings.GetOrDefault<bool>("IsPathAutosaveEnabled");
            LineStippleFactor = Settings.GetOrDefault<int>("LineStippleFactor");
            LineWidth = Settings.GetOrDefault<double>("LineWidth");
            PinSize = Settings.GetOrDefault<double>("PinSize");
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
            ConfirmActionWindow caw = new("Are you sure you want to restore default settings?");
            if (caw.ShowDialog().GetValueOrDefault())
            {
                Settings.RestoreDefault();
                InitializeColorsFromSettings();
                ResetToDefault?.Invoke();
            }
        }

        private static void ChangeColorSetting(string settingName, Color newColor, WriteableBitmap writeableBitmap)
        {
            Settings.Set(settingName, new SolidColorBrush(newColor));
            BitmapColorChanger.ChangeColorKeepAlpha(writeableBitmap, newColor);
        }
    }
}
