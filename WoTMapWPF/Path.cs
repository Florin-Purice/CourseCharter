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
        private MapManagerService? mapManagerService;

        /// <summary>
        /// Constructor for Path. Must call SetMapManager before using.
        /// Used for deserializing
        /// </summary>
        public Path()
        {
            Nodes = [];
            TotalDistance = 0;
            SelectedIndex = -1;
        }

        public Path(MapManagerService mapManagerService)
        {
            Nodes = [];
            TotalDistance = 0;
            SelectedIndex = -1;
            this.mapManagerService = mapManagerService;
        }

        private Path(Path path)
        {
            mapManagerService = path.mapManagerService;
            Nodes = [];
            TotalDistance = path.TotalDistance;
            SelectedIndex = path.SelectedIndex;
            foreach (PathNode node in path.Nodes)
                Nodes.Add((PathNode)node.Clone());
        }

        public event EventHandler? PathChanged;

        [ObservableProperty]
        public partial double TotalDistance { get; set; }
        [ObservableProperty]
        public partial int SelectedIndex { get; set; }
        [ObservableProperty]
        public partial List<PathNode> Nodes { get; set; }

        public object Clone()
        {
            return new Path(this);
        }

        partial void OnNodesChanged(List<PathNode> value)
        {
            UnsubFromNodes();
            SubToNodes();
        }

        public void SetMapManager(MapManagerService mapManagerService) => this.mapManagerService = mapManagerService;

        public void OnMoveFinished()
        {
            PathChanged?.Invoke(this, EventArgs.Empty);
        }

        public void InsertNode(int index, PathNode node)
        {
            Nodes.Insert(index, node);
            OnNodeListChanged();
        }

        public void RemoveNode(int index)
        {
            Nodes.RemoveAt(index);
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
            CollectionViewSource.GetDefaultView(Nodes).Refresh();
        }

        private void OnNodeListChanged()
        {
            ComputeRelativeNodeData();
            PathChanged?.Invoke(this, EventArgs.Empty);
            CollectionViewSource.GetDefaultView(Nodes).Refresh();
        }

        private void ComputeRelativeNodeData()
        {
            //compute all relative node data:
            if (Nodes.Count > 0 && mapManagerService?.Map != null)
            {
                UnsubFromNodes();
                double pixelsPerUnit = mapManagerService.Map.HeightP / Scene.VERTICAL_UNITS;
                double compoundDistance = 0;
                //distance, hasdistance, isnamed, index
                for (int i = 0; i < Nodes.Count - 1; i++)
                {
                    double xuDiff = Nodes[i + 1].Position.X - Nodes[i].Position.X;
                    double yuDiff = Nodes[i + 1].Position.Y - Nodes[i].Position.Y;
                    double unitDistance = Math.Sqrt(xuDiff * xuDiff + yuDiff * yuDiff);
                    Nodes[i].Distance = unitDistance * pixelsPerUnit;
                    compoundDistance += Nodes[i].Distance;
                    Nodes[i].HasDistance = true;
                    Nodes[i].Index = i;
                    Nodes[i].IsNamed = !string.IsNullOrEmpty(Nodes[i].Name);
                }
                Nodes.First().IsNamed = true;
                Nodes.Last().IsNamed = true;
                Nodes.Last().Distance = 0;
                Nodes.Last().HasDistance = false;
                Nodes.Last().Index = Nodes.Count - 1;
                TotalDistance = compoundDistance;
                //compounddistance
                compoundDistance = 0;
                if (Nodes[0].HasDistance)
                    compoundDistance = Nodes[0].Distance;
                PathNode lastNamedNode = Nodes[0];
                for (int i = 1; i < Nodes.Count - 1; i++)
                {
                    if (Nodes[i].IsNamed)
                    {
                        lastNamedNode.CompoundDistance = compoundDistance;
                        if (Nodes[i].HasDistance)
                            compoundDistance = Nodes[i].Distance;
                        lastNamedNode = Nodes[i];
                    }
                    else if (Nodes[i].HasDistance)
                        compoundDistance += Nodes[i].Distance;
                }
                lastNamedNode.CompoundDistance = compoundDistance;
                SubToNodes();
            }
        }

        private void UnsubFromNodes()
        {
            foreach (PathNode node in Nodes)
                node.ClearPropertyChangedEvent();
        }

        private void SubToNodes()
        {
            //subscribe too every node propchanged event
            foreach (PathNode node in Nodes)
                node.PropertyChanged += Node_PropertyChanged;
        }
    }
}
