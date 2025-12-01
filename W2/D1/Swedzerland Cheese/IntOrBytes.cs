using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Swedzerland_Cheese
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct IntOrBytes
    {
        [FieldOffset(0)]
        public int IntValue;

        [FieldOffset(0)] // То же самое смещение!
        public byte Byte0;

        [FieldOffset(1)]
        public byte Byte1;

        [FieldOffset(2)]
        public byte Byte2;

        [FieldOffset(3)]
        public byte Byte3;
    }
}
