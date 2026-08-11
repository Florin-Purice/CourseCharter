using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace WoTMapWPF
{
    public class PathNode : ICloneable, INotifyPropertyChanged
    {
        private GLPosition position;
        private int index;
        private string name;
        private double distance;
        private bool hasDistance;
        private bool isNamed;
        private double compoundDistance;

        public PathNode()
        {
            position = new GLPosition();
            index = -1;
            name = string.Empty;
            isNamed = false;
            distance = 0;
            compoundDistance = 0;
            hasDistance = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public object Clone()
        {
            PathNode clone = new PathNode
            {
                Position = (GLPosition)this.Position.Clone(),
                Index = this.Index,
                Name = this.Name,
                IsNamed = this.IsNamed,
                Distance = this.Distance,
                CompoundDistance = this.CompoundDistance,
                HasDistance = this.HasDistance
            };
            return clone;
        }

        /// <summary>
        /// GL position where the marker is drawn
        /// </summary>
        public GLPosition Position { get { return position; } set { position = value; OnPropertyChanged("Position"); } }
        /// <summary>
        /// The index of this node in the path containing it, starting with 0
        /// </summary>
        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        /// <summary>
        /// User defined name
        /// </summary>
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
        /// <summary>
        /// Distance to next path node, in pixels on map image
        /// </summary>
        public double Distance { get { return distance; } set { distance = value; OnPropertyChanged("Distance"); } }
        /// <summary>
        /// True for all path nodes except last
        /// </summary>
        public bool HasDistance { get { return hasDistance; } set { hasDistance = value; OnPropertyChanged("HasDistance"); } }
        /// <summary>
        /// True if has user defined name. Also true for first and last node
        /// </summary>
        public bool IsNamed { get { return isNamed; } set { isNamed = value; OnPropertyChanged("IsNamed"); } }
        /// <summary>
        /// Distance in map pixels to next named node
        /// </summary>
        public double CompoundDistance { get { return compoundDistance; } set { compoundDistance = value; OnPropertyChanged("CompoundDistance"); } }

        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return $"Node {Index}";
                else
                    return Name;
            }
            set
            {
                Name = value;
            }
        }

        public void ClearPropertyChangedEvent()
        {
            PropertyChanged = null;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class GLPosition : ICloneable
        {
            public GLPosition()
            {
                X = 0; Y = 0;
            }

            public GLPosition(float x, float y)
            {
                X = x; Y = y;
            }

            public object Clone()
            {
                return new GLPosition(X, Y);
            }

            public float X { get; set; }
            public float Y { get; set; }
        }
    }
}
