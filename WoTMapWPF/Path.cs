using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using WoTMapWPF.Graphics;
using WoTMapWPF.Services;

namespace WoTMapWPF
{
    public partial class Path : ObservableObject, ICloneable
    {
        private List<PathNode> nodes;
        private readonly MapManagerService mapManagerService;

        public Path(MapManagerService mapManagerService)
        {
            nodes = new List<PathNode>();
            TotalDistance = 0;
            SelectedIndex = -1;
            this.mapManagerService = mapManagerService;
        }

        private Path(Path path)
        {
            mapManagerService = path.mapManagerService;
            nodes = new List<PathNode>();
            TotalDistance = path.TotalDistance;
            SelectedIndex = path.SelectedIndex;
            foreach (PathNode node in path.Nodes)
                nodes.Add((PathNode)node.Clone());
            SubToNodes();
        }

        public event EventHandler? PathChanged;

        /// <summary>
        /// Total path distance in map image pixels
        /// </summary>
        [ObservableProperty]
        public partial double TotalDistance { get; set; }

        /// <summary>
        /// Index of the selected path node
        /// </summary>
        [ObservableProperty]
        public partial int SelectedIndex { get; set; }

        public List<PathNode> Nodes
        {
            get { return nodes; }
            set
            {
                nodes = value;
                UnsubFromNodes();
                SubToNodes();
                OnPropertyChanged("Nodes");
            }
        }

        public object Clone()
        {
            return new Path(this);
        }

        public void OnMoveFinished()
        {
            PathChanged?.Invoke(this, EventArgs.Empty);
        }

        public void InsertNode(int index, PathNode node)
        {
            nodes.Insert(index, node);
            OnNodeListChanged();
        }

        public void RemoveNode(int index)
        {
            nodes.RemoveAt(index);
            OnNodeListChanged();
        }

        private void Node_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            //if node position changed recompute relative node data, same if name changed - isnamed calculation
            if (e.PropertyName == "Position" || e.PropertyName == "Name")
                ComputeRelativeNodeData();
            //path "changed" when node Name changes or when a node move is finished
            if (e.PropertyName == "Name")
                PathChanged?.Invoke(this, EventArgs.Empty);
            CollectionViewSource.GetDefaultView(nodes).Refresh();
        }

        private void OnNodeListChanged()
        {
            ComputeRelativeNodeData();
            PathChanged?.Invoke(this, EventArgs.Empty);
            CollectionViewSource.GetDefaultView(nodes).Refresh();
        }

        private void ComputeRelativeNodeData()
        {
            //compute all relative node data:
            if (nodes.Count > 0 && mapManagerService.Map != null)
            {
                UnsubFromNodes();
                double pixelsPerUnit = mapManagerService.Map.HeightP / Scene.VERTICAL_UNITS;
                double compoundDistance = 0;
                //distance, hasdistance, isnamed, index
                for (int i = 0; i < nodes.Count - 1; i++)
                {
                    double xuDiff = nodes[i + 1].Position.X - nodes[i].Position.X;
                    double yuDiff = nodes[i + 1].Position.Y - nodes[i].Position.Y;
                    double unitDistance = Math.Sqrt(xuDiff * xuDiff + yuDiff * yuDiff);
                    nodes[i].Distance = unitDistance * pixelsPerUnit;
                    compoundDistance += nodes[i].Distance;
                    nodes[i].HasDistance = true;
                    nodes[i].Index = i;
                    nodes[i].IsNamed = !string.IsNullOrEmpty(nodes[i].Name);
                }
                nodes.First().IsNamed = true;
                nodes.Last().IsNamed = true;
                nodes.Last().Distance = 0;
                nodes.Last().HasDistance = false;
                nodes.Last().Index = nodes.Count - 1;
                TotalDistance = compoundDistance;
                //compounddistance
                compoundDistance = 0;
                if (nodes[0].HasDistance)
                    compoundDistance = nodes[0].Distance;
                PathNode lastNamedNode = nodes[0];
                for (int i = 1; i < nodes.Count - 1; i++)
                {
                    if (nodes[i].IsNamed)
                    {
                        lastNamedNode.CompoundDistance = compoundDistance;
                        if (nodes[i].HasDistance)
                            compoundDistance = nodes[i].Distance;
                        lastNamedNode = nodes[i];
                    }
                    else if (nodes[i].HasDistance)
                        compoundDistance += nodes[i].Distance;
                }
                lastNamedNode.CompoundDistance = compoundDistance;
                SubToNodes();
            }
        }

        private void UnsubFromNodes()
        {
            foreach (PathNode node in nodes)
                node.ClearPropertyChangedEvent();
        }

        private void SubToNodes()
        {
            //subscribe too every node propchanged event
            foreach (PathNode node in nodes)
                node.PropertyChanged += Node_PropertyChanged;
        }
    }
}
