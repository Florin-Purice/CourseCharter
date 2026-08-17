using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace WoTMapWPF
{
    public partial class MapFileDefinition : ObservableObject
    {
        public string Name { get; set; } = string.Empty;
        public string UnitLabel { get; set; } = string.Empty;
        public string ImageMD5 { get; set; } = string.Empty;
        public string ImageExt { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DistanceUnitsPerPixel))]
        public partial int SampleUnits { get; set; }
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DistanceUnitsPerPixel))]
        public partial int SamplePixels { get; set; }
        [JsonIgnore]
        public double DistanceUnitsPerPixel => (double)SampleUnits / SamplePixels;

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not MapFileDefinition)
                return false;
            MapFileDefinition other = (MapFileDefinition)obj;
            if (Name == other.Name &&
                ImageMD5 == other.ImageMD5 &&
                SamplePixels == other.SamplePixels &&
                SampleUnits == other.SampleUnits &&
                UnitLabel == other.UnitLabel &&
                ImageExt == other.ImageExt)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
