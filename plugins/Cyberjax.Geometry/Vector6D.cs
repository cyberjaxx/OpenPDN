using System;

namespace Cyberjax.Geometry
{
    public class Vector6D : Point6D
    {
        public Vector6D()
        { }

        public Vector6D(Point6D point) :
            base(point)
        { }

        public Vector6D(Vector6D vector) :
            base(vector.Coords)
        { }

        public Vector6D(params double[] values) :
            base(values)
        { }

        public override double this[int i]
        {
            get { return Coords[i]; }
            set { Coords[i] = value; }
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

        public Vector6D Normalize()
        {
            double length = Length;
            Vector6D vector = new Vector6D();
            for (int i = 0; i < Count; i++)
            {
                vector[i] = this[i] / length;
            }
            return vector;
        }

        public static Vector6D Unit
        {
            get { return new Vector6D(1.0); }
        }

        public new Vector6D Clone()
        {
            return new Vector6D(this);
        }


        public static Vector6D operator -(Vector6D v)
        {
            Vector6D vector = new Vector6D();
            for (int i = 0; i < Count; i++)
            {
                vector[i] = -v[i];
            }
            return vector;
        }

        public static Vector6D operator +(Vector6D v1, Vector6D v2)
        {
            Vector6D vector = new Vector6D();
            for (int i = 0; i < Count; i++)
            {
                vector[i] = v1[i] + v2[i];
            }
            return vector;
        }

        public static Vector6D operator +(Vector6D v, Point6D p)
        {
            return v + (Vector6D)p;
        }

        public static Vector6D operator -(Vector6D v1, Vector6D v2)
        {
            return v1 - v2;
        }
    }
}
