
using System;

namespace Cyberjax.Geometry
{
    public class Point8D : IComparable<Point8D>, IComparable, IEpsilonComparable<Point8D>, ICloneable
    {
        public const int _X_ = 0;
        public const int _Y_ = 1;
        public const int _Z_ = 2;
        public const int _A_ = 3;
        public const int _B_ = 4;
        public const int _C_ = 5;
        public const int _U_ = 6;
        public const int _V_ = 7;
        public const int Count = 8;
        public const double UnsetValue = -1.23432101234321E+308;

        protected double[] m_coords = new double[Count];

        public Point8D()
        {
        }

        public Point8D(params double[] Coords)
        {
            Set(Coords);
        }

        public Point8D(double value)
        {
            Set(value);
        }

        public Point8D(Point8D point)
        {
            Set(point.Coords);
        }

        public Point8D(Point3D point)
        {
            Set(point.X, point.Y, point.Z);
        }

        public virtual double this[int i]
        {
            get { return m_coords[i]; }
            set { m_coords[i] = value; }
        }

        public static Point8D Zero
        {
            get { return new Point8D(0.0); }
        }

        public static Point8D NaN
        {
            get { return new Point8D(double.NaN); }
        }

        public static Point8D Unset
        {
            get { return new Point8D(UnsetValue); }
        }

        public bool IsNaN
        {
            get
            {
                for (int i = 0; i < Count; i++)
                {
                    if (!double.IsNaN(Coords[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool IsZero
        {
            get
            {
                for (int i = 0; i < Count; i++)
                {
                    if (0 != Coords[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public static explicit operator Point8D(Point3D point)
        {
            return new Point8D(point);
        }

        public static explicit operator Point3D(Point8D point)
        {
            return new Point3D(point.X, point.Y, point.Z);
        }

        public static Point8D operator -(Point8D point)
        {
            return Negate(point);
        }

        public static Vector8D operator -(Point8D point1, Point8D point2)
        {
            return Subtract(point1, point2);
        }

        public static Point8D operator -(Point8D point, Vector8D vector)
        {
            return Subtract(point, vector);
        }

        public static Point8D operator -(Point8D point, Vector3D vector)
        {
            return Subtract(point, vector);
        }

        public static Point8D operator +(Vector8D vector, Point8D point)
        {
            return Add(point, vector);
        }

        public static Point8D operator +(Point8D point, Vector8D vector)
        {
            return Add(point, vector);
        }

        public static Point8D operator +(Point8D point, Vector3D vector)
        {
            return Add(point, vector);
        }

        public static bool operator ==(Point8D a, Point8D b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Point8D a, Point8D b)
        {
            return !a.Equals(b);
        }

        public static Point8D operator *(Point8D point, double t)
        {
            return Multiply(point, t);
        }

        public static Point8D operator /(Point8D point, double t)
        {
            return Divide(point, t);
        }

        public static bool operator <(Point8D a, Point8D b)
        {
            return -1 == a.CompareTo(b);
        }

        public static bool operator >(Point8D a, Point8D b)
        {
            return 1 == a.CompareTo(b);
        }

        public static bool operator <=(Point8D a, Point8D b)
        {
            return 1 != a.CompareTo(b);
        }

        public static bool operator >=(Point8D a, Point8D b)
        {
            return 1 != a.CompareTo(b);
        }

        public static Point8D Negate(Point8D point)
        {
            Point8D newPoint = new Point8D();
            for (int i = 0; i < Count; i++)
            {
                newPoint[i] = -point[i];
            }
            return newPoint;
        }

        public static Point8D Add(Point8D point, Vector8D vector)
        {
            Point8D newPoint = new Point8D();
            for (int i = 0; i < Count; i++)
            {
                newPoint[i] = point[i] + vector[i];
            }
            return newPoint;
        }

        public static Point8D Add(Point8D point, Vector3D vector)
        {
            Point8D newPoint = new Point8D(point);
            newPoint.X += vector.X;
            newPoint.Y += vector.Y;
            newPoint.Z += vector.Z;
            return newPoint;
        }

        public static Vector8D Subtract(Point8D point1, Point8D point2)
        {
            Vector8D newVector = new Vector8D();
            for (int i = 0; i < Count; i++)
            {
                newVector[i] = point1[i] - point2[i];
            }
            return newVector;
        }

        public static Point8D Subtract(Point8D point, Vector8D vector)
        {
            Point8D newPoint = new Point8D();
            for (int i = 0; i < Count; i++)
            {
                newPoint[i] = point[i] - vector[i];
            }
            return newPoint;
        }

        public static Point8D Subtract(Point8D point, Vector3D vector)
        {
            Point8D newPoint = new Point8D(point);
            newPoint.X -= vector.X;
            newPoint.Y -= vector.Y;
            newPoint.Z -= vector.Z;
            return newPoint;
        }

        public static Point8D Multiply(Point8D point, double t)
        {
            Point8D newPoint = new Point8D();
            for (int i = 0; i < Count; i++)
            {
                newPoint[i] = point[i] * t;
            }
            return newPoint;
        }

        public static Point8D Divide(Point8D point, double t)
        {
            Point8D newPoint = new Point8D();
            for (int i = 0; i < Count; i++)
            {
                newPoint[i] = point[i] / t;
            }
            return newPoint;
        }

        public int CompareTo(Point8D other)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i] < other[i]) { return -1; }
                if (this[i] > other[i]) { return 1; }
            }
            return 0;
        }

        public bool Equals(Point8D point)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i] != point[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (typeof(Point8D) != obj.GetType())
                return false;
            else
                return Equals((Point8D)obj);
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
            return string.Format("({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", X, Y, Z, A, B, C, U, V);
        }

        public Point8D Clone()
        {
            return new Point8D(this.Coords);
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

        public void Set(Point8D point)
        {
            Set(point.Coords);
        }

        int IComparable.CompareTo(object obj)
        {
            {
                if (obj == null)
                    return -1;

                if (typeof(Point8D) != obj.GetType())
                    return -1;
                else
                    return CompareTo((Point8D)obj);
            }
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        bool IEpsilonComparable<Point8D>.EpsilonEquals(Point8D other, double epsilon)
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
            get { return this[_X_]; }
            set { this[_X_] = value; }
        }

        public double Y
        {
            get { return this[_Y_]; }
            set { this[_Y_] = value; }
        }

        public double Z
        {
            get { return this[_Z_]; }
            set { this[_Z_] = value; }
        }

        public double A
        {
            get { return this[_A_]; }
            set { this[_A_] = value; }
        }

        public double B
        {
            get { return this[_B_]; }
            set { this[_B_] = value; }
        }

        public double C
        {
            get { return this[_C_]; }
            set { this[_C_] = value; }
        }

        public double U
        {
            get { return this[_U_]; }
            set { this[_U_] = value; }
        }

        public double V
        {
            get { return this[_V_]; }
            set { this[_V_] = value; }
        }

        public void Merge(Point8D point)
        {
            for (int i = 0; i < Count; i++)
            {
                if (!double.IsNaN(point[i]))
                {
                    this[i] = point[i];
                }
            }
        }

        public void Scale(double t)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i] *= t;
            }
        }

        public void Scale(Vector8D vector)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i] *= vector[i];
            }
        }

        public void Translate(Vector8D vector)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i] += vector[i];
            }
        }
    }
}

