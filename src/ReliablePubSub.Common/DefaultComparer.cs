using System;
using System.Collections;
using System.Collections.Generic;

namespace ReliablePubSub.Common
{
    public class DefaultComparer<T> : IComparer<T>, IComparer where T : IComparable<T>
    {
        public int Compare(T x, T y)
        {
            return x != null ? x.CompareTo(y) : 0;
        }

        public int Compare(object x, object y)
        {
            return Compare((T)x, (T)y);
        }
    }
}
