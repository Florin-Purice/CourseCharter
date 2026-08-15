using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WoTMapWPF.CustomControls
{
    public partial class GLLineStipplePatternControlViewModel : ObservableObject
    {
        public GLLineStipplePatternControlViewModel()
        {
            BitList = [];
            for (int i = 0; i < 16; i++)
                BitList.Add(new StippleBit(false));
            InitializeBitList();
            foreach (StippleBit bit in BitList)
                bit.PropertyChanged += Bit_PropertyChanged;
        }

        [ObservableProperty]
        public partial short StipplePattern { get; set; }

        public List<StippleBit> BitList { get; private set; }

        public void InitializeBitList()
        {
            short stipplePattern = (short)App.Current.Resources["LineStipplePattern"];
            byte[] stippleBytes = BitConverter.GetBytes(stipplePattern);
            int bitIndex = 0;
            foreach (byte b in stippleBytes)
            {
                for (int i = 0; i < 8; i++)
                {
                    bool bitValue = (b & (1 << i)) != 0;
                    BitList[bitIndex++].Value = bitValue;
                }
            }
        }

        private void Bit_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            byte[] stippleBytes = new byte[2];
            int bitIndex = 0;
            int bitValue;
            for (int j = 0; j < stippleBytes.Length; j++)
            {
                for (int i = 0; i < 8; i++)
                {
                    bitValue = BitList[bitIndex++].Value ? 1 : 0;
                    stippleBytes[j] = (byte)(stippleBytes[j] & ~(1 << i) | ((int)bitValue << i));
                }
            }
            StipplePattern = BitConverter.ToInt16(stippleBytes, 0);
        }

        public partial class StippleBit(bool value) : ObservableObject
        {
            [ObservableProperty]
            public partial bool Value { get; set; } = value;
        }
    }
}
