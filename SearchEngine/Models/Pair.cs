using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    /// <summary>
    /// This class holds 2 values
    /// Use to keep result of query (doc ID, angle)
    /// </summary>
    public class Pair <U, V>: IComparable<Pair<U, V>>
        where U: IComparable<U> 
        where V: IComparable<V>
    {
        public U A; // doc id or precision
        public V B; // angle or recall

        public Pair(U _a, V _b)
        {
            this.A = _a;
            this.B = _b;
        }

        public int CompareTo(Pair<U, V> other)
        {
            if (other == null)
                return -1;
            return B.CompareTo(other.B);
        }
    }
}
