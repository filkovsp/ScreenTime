using System;
using System.Collections.Generic;

namespace Windows
{
    /// <summary>
    /// IntPtr cant be compared directly, it must be converted to Int32 first. More info here:
    /// https://stackoverflow.com/questions/21909313/c-sharp-sorting-list-with-byte-array-doesnt-work/21909403
    /// </summary>
    public class IntPtrComparer : IComparer<IntPtr>
    {
        public int Compare(IntPtr x, IntPtr y)
        {
            if (x.ToInt32() < y.ToInt32()) return -1;
            if (x.ToInt32() > y.ToInt32()) return 1;
            return 0;
        }
    }
}