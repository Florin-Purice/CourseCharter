using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WoTMapWPF.CustomControls
{
    /// <summary>
    /// Interaction logic for LineStipple.xaml
    /// </summary>
    public partial class LineStipple : UserControl
    {
        public static readonly DependencyProperty PatternProperty =
            DependencyProperty.Register(nameof(Pattern), typeof(short), typeof(LineStipple),
                new FrameworkPropertyMetadata(
                    defaultValue: (short)0,
                    flags: FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    propertyChangedCallback: OnPatternChangedCallback,
                    coerceValueCallback: null,
                    isAnimationProhibited: true,
                    defaultUpdateSourceTrigger: UpdateSourceTrigger.PropertyChanged));
        public static readonly DependencyProperty BitSizeProperty =
            DependencyProperty.Register(nameof(BitSize), typeof(double), typeof(LineStipple),
                new FrameworkPropertyMetadata(
                    defaultValue: 15d,
                    flags: FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    propertyChangedCallback: null,
                    coerceValueCallback: null,
                    isAnimationProhibited: true,
                    defaultUpdateSourceTrigger: UpdateSourceTrigger.PropertyChanged));
        public static readonly DependencyProperty ColorActiveProperty =
            DependencyProperty.Register(nameof(ColorActive), typeof(SolidColorBrush), typeof(LineStipple), new PropertyMetadata(new SolidColorBrush(Colors.Red)));
        public static readonly DependencyProperty ColorInactiveProperty =
            DependencyProperty.Register(nameof(ColorInactive), typeof(SolidColorBrush), typeof(LineStipple), new PropertyMetadata(new SolidColorBrush(Colors.Gray)));
        private readonly List<StippleBit> bitList;

        public LineStipple()
        {
            InitializeComponent();
            bitList = [];
            for (int i = 0; i < 16; i++)
                bitList.Add(new StippleBit(false));
            PopulateBitContainer();
        }

        public short Pattern
        {
            get { return (short)GetValue(PatternProperty); }
            set { SetValue(PatternProperty, value); }
        }

        public double BitSize
        {
            get { return (double)GetValue(BitSizeProperty); }
            set { SetValue(BitSizeProperty, value); }
        }

        public SolidColorBrush ColorActive
        {
            get { return (SolidColorBrush)GetValue(ColorActiveProperty); }
            set { SetValue(ColorActiveProperty, value); }
        }

        public SolidColorBrush ColorInactive
        {
            get { return (SolidColorBrush)GetValue(ColorInactiveProperty); }
            set { SetValue(ColorInactiveProperty, value); }
        }

        private static void OnPatternChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LineStipple)d).InitializeBits((short)(e.NewValue));
        }

        private void InitializeBits(short value)
        {
            byte[] stippleBytes = BitConverter.GetBytes(value);
            int bitIndex = 0;
            foreach (byte b in stippleBytes)
            {
                for (int i = 0; i < 8; i++)
                {
                    bool bitValue = (b & (1 << i)) != 0;
                    bitList[bitIndex++].Value = bitValue;
                }
            }
        }

        private void PopulateBitContainer()
        {
            Grid bit;
            for (int i = 0; i < 16; i++)
            {
                bit = new Grid();
                Grid.SetColumn(bit, i);
                Binding tagBinding = new("Value") { Source = bitList[i] };
                bit.SetBinding(Grid.TagProperty, tagBinding);
                bit.SetResourceReference(Grid.StyleProperty, "BitStyle");
                bit.PreviewMouseLeftButtonDown += OnBitClick;
                bit.MouseEnter += OnBitMouseEnter;
                BitContainer.Children.Add(bit);
            }
        }

        private void OnBitClick(object sender, MouseButtonEventArgs e)
        {
            Grid bit = (Grid)sender;
            int position = Grid.GetColumn(bit);
            ToggleBit(position);
        }

        private void OnBitMouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Grid bit = (Grid)sender;
                int position = Grid.GetColumn(bit);
                ToggleBit(position);
            }
        }

        private void ToggleBit(int index)
        {
            if (index < 0 && index > 15)
                return;
            bitList[index].Value = !bitList[index].Value;
            OnBitListChanged();
        }

        private void OnBitListChanged()
        {
            byte[] stippleBytes = new byte[2];
            int bitIndex = 0;
            int bitValue;
            for (int j = 0; j < stippleBytes.Length; j++)
            {
                for (int i = 0; i < 8; i++)
                {
                    bitValue = bitList[bitIndex++].Value ? 1 : 0;
                    stippleBytes[j] = (byte)(stippleBytes[j] & ~(1 << i) | (bitValue << i));
                }
            }
            Pattern = BitConverter.ToInt16(stippleBytes, 0);
        }

        private partial class StippleBit(bool value) : ObservableObject
        {
            [ObservableProperty]
            public partial bool Value { get; set; } = value;
        }
    }
}
