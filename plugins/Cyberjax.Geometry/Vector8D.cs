using System;

namespace Cyberjax.Geometry
{
    public class Vector8D : Point8D
    {
        public Vector8D()
        { }

        public Vector8D(Point8D point) :
            base(point)
        { }

        public Vector8D(Vector8D vector) :
            base(vector.Coords)
        { }

        public Vector8D(double value) :
            base(value)
        { }

        public Vector8D(params double[] values) :
            base(values)
        { }

        public override double this[int i]
        {
            get { return m_coords[i]; }
            set { m_coords[i] = value; }
        }

        public double Length
        {
            get
            {
                double sumSquares = 0.0;
                for (int i = 0; i < Count; i++)
                {
                    double values = this[i];
                    sumSquares += values * values;
                }
                return Math.Sqrt(sumSquares);
            }
        }

        public Vector8D Normalize()
        {
            double length = Length;
            Vector8D vector = new Vector8D();
            for (int i = 0; i < Count; i++)
            {
                vector[i] = this[i] / length;
            }
            return vector;
        }

        public static Vector8D Unit
        {
            get { return new Vector8D(1.0); }
        }

        public static new Vector8D Zero
        {
            get { return new Vector8D(0.0); }
        }

        public new Vector8D Clone()
        {
            return new Vector8D(this);
        }


        public static Vector8D operator -(Vector8D v)
        {
            Vector8D vector = new Vector8D();
            for (int i = 0; i < Count; i++)
            {
                vector[i] = -v[i];
            }
            return vector;
        }

        public static Vector8D operator +(Vector8D v1, Vector8D v2)
        {
            Vector8D vector = new Vector8D();
            for (int i = 0; i < Count; i++)
            {
                vector[i] = v1[i] + v2[i];
            }
            return vector;
        }

        public static Vector8D operator -(Vector8D v1, Vector8D v2)
        {
            return v1 - v2;
        }
    }
}
