using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    public abstract class MyVector
    {
        // Use dictionary to store index:value because there are many many empty elements (sparse)
        // index = index of this value
        // the rest is zero by default
        public Dictionary<int, float> Coordinates;

        public MyVector() { }

        public MyVector(int length)
        {
            Coordinates = new Dictionary<int, float>();
        }

        public virtual float Magnitude()
        {
            float result = 0;
            foreach(var coor in Coordinates.Values)
                result += coor * coor;
            return (float)Math.Sqrt(result);
        }

        public virtual float DotProduct(MyVector other)
        {
            float result = 0;
            foreach(var entry in Coordinates)
                if (other.Coordinates.ContainsKey(entry.Key))
                    result += entry.Value * other.Coordinates[entry.Key];

            return result;
        }

        public virtual float Angle(MyVector other)
        {
            float cos = this.DotProduct(other) / (this.Magnitude() * other.Magnitude());
            return (float)Math.Acos(cos);
        }

    }
}
