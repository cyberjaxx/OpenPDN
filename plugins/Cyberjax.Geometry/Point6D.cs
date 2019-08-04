
using System;

namespace Cyberjax.Geometry
{
    public class Point6D : IComparable<Point6D>, IComparable, IEpsilonComparable<Point6D>, ICloneable
    {
        public const int _X_ = 0;
        public const int _Y_ = 1;
        public const int _Z_ = 2;
        public const int _A_ = 3;
        public const int _B_ = 4;
        public const int _C_ = 5;
        public const int Count = 6;
        public const double UnsetValue = -1.23432101234321E+308;

        protected double[] m_coords = new double[Count];

        public Point6D()
        {
        }

        public Point6D(params double[] Coords)
        {
            Set(Coords);
        }

        public Point6D(double value)
        {
            Set(value);
        }

        public Point6D(Point6D point)
        {
            Set(point.Coords);
        }

        public Point6D(Point3D point)
        {
            Set(point.X, point.Y, point.Z);
        }

        public virtual double this[int i]
        {
            get { return Coords[i]; }
            set { Coords[i] = value; }
        }

        public static Point6D Zero
        {
            get { return new Point6D(0.0); }
        }

        public static explicit operator Point6D(Point3D point)
        {
            return new Point6D(point);
        }

        public static explicit operator Point3D(Point6D point)
        {
            return new Point3D(point.X, point.Y, point.Z);
        }

        public static Point6D operator -(Point6D point)
        {
            return Negate(point);
        }

        public static Vector6D operator -(Point6D point1, Point6D point2)
        {
            return Subtract(point1, point2);
        }

        public static Point6D operator -(Point6D point, Vector6D vector)
        {
            return Subtract(point, vector);
        }

        public static Point6D operator -(Point6D point, Vector3D vector)
        {
            return Subtract(point, vector);
        }

        public static Point6D operator +(Vector6D vector, Point6D point)
        {
            return Add(point, vector);
        }

        public static Point6D operator +(Point6D point, Vector6D vector)
        {
            return Add(point, vector);
        }


        public static Point6D operator +(Point6D point, Vector3D vector)
        {
            return Add(point, vector);
        }

        public static bool operator ==(Point6D a, Point6D b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Point6D a, Point6D b)
        {
            return !a.Equals(b);
        }

        public static Point6D operator *(Point6D point, double t)
        {
            return Multiply(point, t);
        }

        public static Point6D operator /(Point6D point, double t)
        {
            return Divide(point, t);
        }

        public static bool operator <(Point6D a, Point6D b)
        {
            return -1 == a.CompareTo(b);
        }

        public static bool operator >(Point6D a, Point6D b)
        {
            return 1 == a.CompareTo(b);
        }

        public static bool operator <=(Point6D a, Point6D b)
        {
            return 1 != a.CompareTo(b);
        }

        public static bool operator >=(Point6D a, Point6D b)
        {
            return 1 != a.CompareTo(b);
        }

        public static Point6D Negate(Point6D point)
        {
            Point6D newPoint = new Point6D();
            for (int i = 0; i < Count; i++)
            {
                newPoint[i] = -point[i];
            }
            return newPoint;
        }

        public static Point6D Add(Point6D point, Vector6D vector)
        {
            Point6D newPoint = new Point6D();
            for (int i = 0; i < Count; i++)
            {
                newPoint[i] = point[i] + vector[i];
            }
            return newPoint;
        }

        public static Point6D Add(Point6D point, Vector3D vector)
        {
            Point6D newPoint = new Point6D(point);
            newPoint.X += vector.X;
            newPoint.Y += vector.Y;
            newPoint.Z += vector.Z;
            return newPoint;
        }

        public static Vector6D Subtract(Point6D point1, Point6D point2)
        {
            Vector6D newVector = new Vector6D();
            for (int i = 0; i < Count; i++)
            {
                newVector[i] = point1[i] - point2[i];
            }
            return newVector;
        }

        public static Point6D Subtract(Point6D point, Vector6D vector)
        {
            Point6D newPoint = new Point6D();
            for (int i = 0; i < Count; i++)
            {
                newPoint[i] = point[i] - vector[i];
            }
            return newPoint;
        }

        public static Point6D Subtract(Point6D point, Vector3D vector)
        {
            Point6D newPoint = new Point6D(point);
            newPoint.X -= vector.X;
            newPoint.Y -= vector.Y;
            newPoint.Z -= vector.Z;
            return newPoint;
        }

        public static Point6D Multiply(Point6D point, double t)
        {
            Point6D newPoint = new Point6D();
            for (int i = 0; i < Count; i++)
            {
                newPoint[i] = point[i] * t;
            }
            return newPoint;
        }

        public static Point6D Divide(Point6D point, double t)
        {
            Point6D newPoint = new Point6D();
            for (int i = 0; i < Count; i++)
            {
                newPoint[i] = point[i] / t;
            }
            return newPoint;
        }

        public int CompareTo(Point6D other)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i] < other[i]) { return -1; }
                if (this[i] > other[i]) { return 1; }
            }
            return 0;
        }

        public bool Equals(Point6D point)
        {
            return this.X == point.X && this.Y == point.Y && this.Z == point.Z &&
                this.A == point.A && this.B == point.B && this.C == point.C;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (typeof(Point6D) != obj.GetType())
                return false;
            else
                return Equals((Point6D)obj);
        }

        public override int GetHashCode()
        {
            int hash = this[0].GetHashCode();
            for (int i = 1; i < Count; i++)
            {
                hash ^= this[i].GetHashCode();
            }
            return hash;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3}, {4}, {5})", X, Y, Z, A, B, C);
        }

        public Point6D Clone()
        {
            return new Point6D(this.Coords);
        }

        public void Set(params double[] Coords)
        {
            int length = Coords.Length;

            if (length <= Count)
            {
                for (int i = 0; i < length; i++)
                {
                    this[i] = Coords[i];
                }
                for (int i = length; i < Count; i++)
                {
                    this[i] = 0.0;
                }
            }
        }

        public void Set(double value)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i] = value;
            }
        }

        public void Set(Point3D point)
        {
            Set(point.X, point.Y, point.Z);
        }

        public void Set(Point6D point)
        {
            Set(point.Coords);
        }

        int IComparable.CompareTo(object obj)
        {
            {
                if (obj == null)
                    return -1;

                if (typeof(Point6D) != obj.GetType())
                    return -1;
                else
                    return CompareTo((Point6D)obj);
            }
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        bool IEpsilonComparable<Point6D>.EpsilonEquals(Point6D other, double epsilon)
        {
            for (int i = 0; i < Count; i++)
            {
                double delta = this[i] - other[i];
                if (delta < -epsilon || delta > epsilon) { return false; }
            }
            return true;
        }

        public double[] Coords
        {
            get { return this.m_coords; }
            set { Set(m_coords); }
        }

        public double X
        {
            get { return Coords[_X_]; }
            set { Coords[_X_] = value; }
        }

        public double Y
        {
            get { return Coords[_Y_]; }
            set { Coords[_Y_] = value; }
        }

        public double Z
        {
            get { return Coords[_Z_]; }
            set { Coords[_Z_] = value; }
        }

        public double A
        {
            get { return Coords[_A_]; }
            set { Coords[_A_] = value; }
        }

        public double B
        {
            get { return Coords[_B_]; }
            set { Coords[_B_] = value; }
        }

        public double C
        {
            get { return Coords[_C_]; }
            set { Coords[_C_] = value; }
        }
    }
}

