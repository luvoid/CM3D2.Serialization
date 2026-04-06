using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CM3D2.Serialization.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IntPair
    {
        public int key;
        public int value;

        public static implicit operator KeyValuePair<int, int>(IntPair intPair)
        {
            return new KeyValuePair<int, int>(intPair.key, intPair.value);
        }

        public static implicit operator IntPair(KeyValuePair<int, int> keyValuePair)
        {
            return new KeyValuePair<int, int>(keyValuePair.Key, keyValuePair.Value);
        }
    }
}
