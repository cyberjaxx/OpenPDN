using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Cyberjax.Geometry
{
    //
    // Summary:
    //     Represents the three coordinates of a point in three-dimensional space, using
    //     System.Double-precision floating point values.
    [DebuggerDisplay("({X}, {Y}, {Z})")]

    public struct Point3D : ISerializable, IEquatable<Point3D>, IComparable<Point3D>, IComparable, IEpsilonComparable<Point3D>, ICloneable
    {
        public const double UnsetValue = -1.23432101234321E+308;
        public const double ZeroTolerance = 1E+12;
        public const double InchesPerMM = 0.039370078740157477;
        public const double MMsPerInch = 25.4;

        //
        // Summary:
        //     Initializes a new point by copying coordinates from the components of a vector.
        //
        // Parameters:
        //   vector:
        //     A vector.
        public Point3D(Vector3D vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }
        //
        // Summary:
        //     Initializes a new point by copying coordinates from a single-precision point.
        //
        // Parameters:
        //   point:
        //     A point.
        //**** public Point3D(Point3f point);
        //
        // Summary:
        //     Initializes a new point by copying coordinates from another point.
        //
        // Parameters:
        //   point:
        //     A point.
        public Point3D(Point3D point)
        {
            X = point.X;
            Y = point.Y;
            Z = point.Z;
        }
        //
        // Summary:
        //     Initializes a new point by copying coordinates from a four-dimensional point.
        //     The first three coordinates are divided by the last one. If the W (fourth) dimension
        //     of the input point is zero, then it will be just discarded.
        //
        // Parameters:
        //   point:
        //     A point.
        //**** public Point3D(Point4d point);
        //
        // Summary:
        //     Initializes a new point by defining the X, Y and Z coordinates.
        //
        // Parameters:
        //   x:
        //     The value of the X (first) coordinate.
        //
        //   y:
        //     The value of the Y (second) coordinate.
        //
        //   z:
        //     The value of the Z (third) coordinate.
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        //
        // Summary:
        //     Gets or sets an indexed coordinate of this point.
        //
        // Parameters:
        //   index:
        //     The coordinate index. Valid values are:
        //     0 = X coordinate
        //     1 = Y coordinate
        //     2 = Z coordinate
        //     .
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    default:
                        throw new ArgumentOutOfRangeException("index", index, "Index must be 0, 1, or 2");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("index", index, "Index must be 0, 1, or 2");
                }
            }
        }
        //
        // Summary:
        //     Gets the value of a point at location UnsetValue,UnsetValue,UnsetValue.
        public static Point3D Unset
        {
            get { return new Point3D(Point3D.UnsetValue, Point3D.UnsetValue, Point3D.UnsetValue); }
        }
        //
        // Summary:
        //     Gets the value of a point at location 0,0,0.
        public static Point3D Origin
        {
            get { return new Point3D(0.0, 0.0, 0.0); }
        }
        //
        // Summary:
        //     Each coordinate of the point must pass the Rhino.IsValidDouble(System.Double)
        //     test.
        public bool IsValid
        {
            get { return !double.IsNaN(X) && !double.IsNaN(Y) && !double.IsNaN(Z); }
        }
        //
        // Summary:
        //     Gets or sets the Z (third) coordinate of this point.
        public double Z { get; set; }
        //
        // Summary:
        //     Gets or sets the Y (second) coordinate of this point.
        public double Y { get; set; }
        //
        // Summary:
        //     Gets or sets the X (first) coordinate of this point.
        public double X { get; set; }
        //
        // Summary:
        //     Gets the smallest (both positive and negative) coordinate value in this point.
        public double MinimumCoordinate
        {
            get
            {
                return Math.Abs(X) > Math.Abs(Y) ?
                  (Math.Abs(Z) > Math.Abs(X) ? Z : X) :
                  (Math.Abs(Z) > Math.Abs(Y) ? Z : Y);
            }
        }
        //
        // Summary:
        //     Gets the largest (both positive and negative) valid coordinate in this point,
        //     or UnsetValue if no coordinate is valid.
        public double MaximumCoordinate
        {
            get
            {
                return Math.Abs(X) < Math.Abs(Y) ?
                  (Math.Abs(Z) < Math.Abs(X) ? Z : X) :
                  (Math.Abs(Z) < Math.Abs(Y) ? Z : Y);
            }
        }
        //
        // Summary:
        //     Sums up a point and a vector, and returns a new point.
        //     (Provided for languages that do not suppPoint3Dort operator overloading. You can use
        //     the + operator otherwise)
        //
        // Parameters:
        //   point:
        //     A point.
        //
        //   vector:
        //     A vector.
        //
        // Returns:
        //     A new point that results from the addition of point and vector.
        public static Point3D Add(Point3D point, Vector3D vector)
        {
            return new Point3D(point.X + vector.X, point.Y + vector.Y, point.Z + vector.Z);
        }
        //
        // Summary:
        //     Sums up a point and a vector, and returns a new point.
        //     (Provided for languages that do not support operator overloading. You can use
        //     the + operator otherwise)
        //
        // Parameters:
        //   point:
        //     A point.
        //
        //   vector:
        //     A vector.
        //
        // Returns:
        //     A new point that results from the addition of point and vector.
        //**** public static Point3D Add(Point3D point, Vector3f vector);
        //
        // Summary:
        //     Sums up a point and a vector, and returns a new point.
        //     (Provided for languages that do not support operator overloading. You can use
        //     the + operator otherwise)
        //
        // Parameters:
        //   vector:
        //     A vector.
        //
        //   point:
        //     A point.
        //
        // Returns:
        //     A new point that results from the addition of point and vector.
        public static Point3D Add(Vector3D vector, Point3D point)
        {
            return Add(point, vector);
        }
        //
        // Summary:
        //     Sums two Point3D instances.
        //     (Provided for languages that do not support operator overloading. You can use
        //     the + operator otherwise)
        //
        // Parameters:
        //   point1:
        //     A point.
        //
        //   point2:
        //     A point.
        //
        // Returns:
        //     A new point that results from the addition of point1 and point2.
        public static Point3D Add(Point3D point1, Point3D point2)
        {
            return new Point3D(point1.X + point2.X, point1.Y + point2.Y, point1.Z + point2.Z);
        }
        //
        // Summary:
        //     Determines whether a set of points is coplanar within a given tolerance.
        //
        // Parameters:
        //   points:
        //     A list, an array or any enumerable of Point3D.
        //
        //   tolerance:
        //     A tolerance value. A default might be ZeroTolerance.
        //
        // Returns:
        //     true if points are on the same plane; false otherwise.
        //**** public static bool ArePointsCoplanar(IEnumerable<Point3D> points, double tolerance);
        //
        // Summary:
        //     Removes duplicates in the supplied set of points.
        //
        // Parameters:
        //   points:
        //     A list, an array or any enumerable of Point3D.
        //
        //   tolerance:
        //     The minimum distance between points.
        //     Points that fall within this tolerance will be discarded.
        //     .
        //
        // Returns:
        //     An array of points without duplicates; or null on error.
        public static Point3D[] CullDuplicates(IEnumerable<Point3D> points, double tolerance)
        {
            List<Point3D> listPoints = new List<Point3D>();
            foreach (Point3D point in points)
            {
                if (listPoints.FindIndex(p => point.DistanceTo(p) < tolerance) < 0)
                {
                    listPoints.Add(point);
                }
            }

            Point3D[] pointArray = new Point3D[listPoints.Count];
            int index = 0;
            foreach (Point3D point in listPoints)
            {
                pointArray[index++] = new Point3D(point);
            }
            return pointArray;
        }
        //
        // Summary:
        //     Divides a Point3D by a number.
        //     (Provided for languages that do not support operator overloading. You can use
        //     the / operator otherwise)
        //
        // Parameters:
        //   point:
        //     A point.
        //
        //   t:
        //     A number.
        //
        // Returns:
        //     A new point that is coordinatewise divided by t.
        public static Point3D Divide(Point3D point, double t)
        {
            return new Point3D(point.X / t, point.Y / t, point.Z / t);
        }
        //
        // Summary:
        //     Converts a single-precision point in a double-precision point.
        //
        // Parameters:
        //   point:
        //     A point.
        //
        // Returns:
        //     The resulting point.
        //**** public static Point3D FromPoint3f(Point3f point);
        //
        // Summary:
        //     Multiplies a Point3D by a number.
        //     (Provided for languages that do not support operator overloading. You can use
        //     the * operator otherwise)
        //
        // Parameters:
        //   point:
        //     A point.
        //
        //   t:
        //     A number.
        //
        // Returns:
        //     A new point that is coordinatewise multiplied by t.
        public static Point3D Multiply(Point3D point, double t)
        {
            return new Point3D(point.X * t, point.Y * t, point.Z * t);
        }
        //
        // Summary:
        //     Multiplies a Point3D by a number.
        //     (Provided for languages that do not support operator overloading. You can use
        //     the * operator otherwise)
        //
        // Parameters:
        //   point:
        //     A point.
        //
        //   t:
        //     A number.
        //
        // Returns:
        //     A new point that is coordinatewise multiplied by t.
        public static Point3D Multiply(double t, Point3D point)
        {
            return Multiply(point, t);
        }
        //
        // Summary:
        //     Orders a set of points so they will be connected in a "reasonable polyline" order.
        //     Also, removes points from the list if their common distance exceeds a specified
        //     threshold.
        //
        // Parameters:
        //   points:
        //     A list, an array or any enumerable of Point3D.
        //
        //   minimumDistance:
        //     Minimum allowed distance among a pair of points. If points are closer than this,
        //     only one of them will be kept.
        //
        // Returns:
        //     The new array of sorted and culled points.
        public static Point3D[] SortAndCullPointList(IEnumerable<Point3D> points, double minimumDistance)
        {
            Point3D[] pointArray = CullDuplicates(points, minimumDistance);
            Array.Sort<Point3D>(pointArray);
            return pointArray;
        }
        //
        // Summary:
        //     Subtracts a point from another point.
        //     (Provided for languages that do not support operator overloading. You can use
        //     the - operator otherwise)
        //
        // Parameters:
        //   point1:
        //     A point.
        //
        //   point2:
        //     Another point.
        //
        // Returns:
        //     A new vector that is the difference of point minus vector.
        public static Vector3D Subtract(Point3D point1, Point3D point2)
        {
            return new Vector3D(point1.X - point2.X, point1.Y - point2.Y, point1.Z - point2.Z);
        }
        //
        // Summary:
        //     Subtracts a vector from a point.
        //     (Provided for languages that do not support operator overloading. You can use
        //     the - operator otherwise)
        //
        // Parameters:
        //   vector:
        //     A vector.
        //
        //   point:
        //     A point.
        //
        // Returns:
        //     A new point that is the difference of point minus vector.
        public static Point3D Subtract(Point3D point, Vector3D vector)
        {
            return new Point3D(point.X - vector.X, point.Y - vector.Y, point.Z - vector.Z);
        }
        //
        // Summary:
        //     Compares this Point3D with another Point3D.
        //     Component evaluation priority is first X, then Y, then Z.
        //
        // Parameters:
        //   other:
        //     The other Point3D to use in comparison.
        //
        // Returns:
        //     0: if this is identical to other
        //     -1: if this.X < other.X
        //     -1: if this.X == other.X and this.Y < other.Y
        //     -1: if this.X == other.X and this.Y == other.Y and this.Z < other.Z
        //     +1: otherwise.
        public int CompareTo(Point3D other)
        {
            if (this.X < other.X) { return -1; }
            if (this.X > other.X) { return 1; }
            if (this.Y < other.Y) { return -1; }
            if (this.Y > other.Y) { return 1; }
            if (this.Z < other.Z) { return -1; }
            if (this.Z > other.Z) { return 1; }
            return 0;
        }
        //
        // Summary:
        //     Computes the distance between two points.
        //
        // Parameters:
        //   other:
        //     Other point for distance measurement.
        //
        // Returns:
        //     The length of the line between this and the other point; or 0 if any of the points
        //     is not valid.
        public double DistanceTo(Point3D other)
        {
            double dx = this.X - other.X;
            double dy = this.Y - other.Y;
            double dz = this.Z - other.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        //
        // Summary:
        //     Computes the square of the distance between two points.
        //     This method is usually largely faster than DistanceTo().
        //
        // Parameters:
        //   other:
        //     Other point for squared distance measurement.
        //
        // Returns:
        //     The squared length of the line between this and the other point; or 0 if any
        //     of the points is not valid.
        public double DistanceToSquared(Point3D other)
        {
            double dx = this.X - other.X;
            double dy = this.Y - other.Y;
            double dz = this.Z - other.Z;
            return dx * dx + dy * dy + dz * dz;
        }
        //
        // Summary:
        //     Check that all values in other are within epsilon of the values in this
        //
        // Parameters:
        //   other:
        //
        //   epsilon:
        public bool EpsilonEquals(Point3D other, double epsilon)
        {
            double dX = this.X - other.X;
            if (dX < -epsilon || dX > epsilon) { return false; }
            double dY = this.Y - other.Y;
            if (dY < -epsilon || dY > epsilon) { return false; }
            double dZ = this.Z - other.Z;
            if (dZ < -epsilon || dZ > epsilon) { return false; }
            return true;
        }
        //
        // Summary:
        //     Determines whether the specified Point3D has the same values as
        //     the present point.
        //
        // Parameters:
        //   point:
        //     The specified point.
        //
        // Returns:
        //     true if point has the same coordinates as this; otherwise false.
        public bool Equals(Point3D point)
        {
            return this.X == point.X && this.Y == point.Y && this.Z == point.Z;
        }
        //
        // Summary:
        //     Determines whether the specified System.Object is a Point3D and
        //     has the same values as the present point.
        //
        // Parameters:
        //   obj:
        //     The specified object.
        //
        // Returns:
        //     true if obj is a Point3D and has the same coordinates as this; otherwise false.
        public override bool Equals(object obj)
        {
            return (obj is Point3D point) ? Equals(point) : false;
        }
        //
        // Summary:
        //     Computes a hash code for the present point.
        //
        // Returns:
        //     A non-unique integer that represents this point.
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }
        //
        // Summary:
        //     Interpolate between two points.
        //
        // Parameters:
        //   pA:
        //     First point.
        //
        //   pB:
        //     Second point.
        //
        //   t:
        //     Interpolation parameter. If t=0 then this point is set to pA. If t=1 then this
        //     point is set to pB. Values of t in between 0.0 and 1.0 result in points between
        //     pA and pB.
        public void Interpolate(Point3D pA, Point3D pB, double t)
        {
            double dx = pB.X - pA.X;
            double dy = pB.Y - pA.Y;
            double dz = pB.Z - pA.Z;
            X = pA.X + dx * t;
            Y = pA.Y + dy * t;
            Z = pA.Z + dz * t;
        }
        //
        // Summary:
        //     Constructs the string representation for the current point.
        //
        // Returns:
        //     The point representation in the form X,Y,Z.
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", X, Y, Z);
        }
        //
        // Summary:
        //     Transforms the present point in place. The transformation matrix acts on the
        //     left of the point. i.e.,
        //     result = transformation*point
        //
        // Parameters:
        //   xform:
        //     Transformation to apply.
        public void Transform(Transform3D xform)
        {
            double x = xform.M00 * X + xform.M01 * Y + xform.M02 * Z + xform.M03;
            double y = xform.M10 * X + xform.M11 * Y + xform.M12 * Z + xform.M13;
            double z = xform.M20 * X + xform.M21 * Y + xform.M22 * Z + xform.M23;

            X = Clamp(x, -4096, 4096);
            Y = Clamp(y, -4096, 4096);
            Z = Clamp(z, -4096, 4096);

        }
        //
        // Summary:
        //     Sums up a point and a vector, and returns a new point.
        //
        // Parameters:
        //   point:
        //     A point.
        //
        //   vector:
        //     A vector.
        //
        // Returns:
        //     A new point that results from the addition of point and vector.
        //**** public static Point3D operator +(Point3D point, Vector3f vector);
        //
        // Summary:
        //     Sums up a point and a vector, and returns a new point.
        //
        // Parameters:
        //   point:
        //     A point.
        //
        //   vector:
        //     A vector.
        //
        // Returns:
        //     A new point that results from the addition of point and vector.
        public static Point3D operator +(Point3D point, Vector3D vector)
        {
            return Add(point, vector);
        }
        //
        // Summary:
        //     Sums two Point3D instances.
        //
        // Parameters:
        //   point1:
        //     A point.
        //
        //   point2:
        //     A point.
        //
        // Returns:
        //     A new point that results from the addition of point1 and point2.
        public static Point3D operator +(Point3D point1, Point3D point2)
        {
            return Add(point1, point2);
        }
        //
        // Summary:
        //     Sums up a point and a vector, and returns a new point.
        //
        // Parameters:
        //   vector:
        //     A vector.
        //
        //   point:
        //     A point.
        //
        // Returns:
        //     A new point that results from the addition of point and vector.
        public static Point3D operator +(Vector3D vector, Point3D point)
        {
            return Add(point, vector);
        }
        //
        // Summary:
        //     Computes the additive inverse of all coordinates in the point, and returns the
        //     new point.
        //
        // Parameters:
        //   point:
        //     A point.
        //
        // Returns:
        //     A point value that, when summed with the point input, yields the Point3D.Origin.
        public static Point3D operator -(Point3D point)
        {
            return new Point3D(-point.X, -point.Y, -point.Z);
        }
        //
        // Summary:
        //     Subtracts a point from another point.
        //
        // Parameters:
        //   point1:
        //     A point.
        //
        //   point2:
        //     Another point.
        //
        // Returns:
        //     A new vector that is the difference of point minus vector.
        public static Vector3D operator -(Point3D point1, Point3D point2)
        {
            return Subtract(point1, point2);
        }
        //
        // Summary:
        //     Subtracts a vector from a point.
        //
        // Parameters:
        //   point:
        //     A point.
        //
        //   vector:
        //     A vector.
        //
        // Returns:
        //     A new point that is the difference of point minus vector.
        public static Point3D operator -(Point3D point, Vector3D vector)
        {
            return Subtract(point, vector);
        }
        //
        // Summary:
        //     Multiplies a Point3D by a number.
        //
        // Parameters:
        //   point:
        //     A point.
        //
        //   t:
        //     A number.
        //
        // Returns:
        //     A new point that is coordinatewise multiplied by t.
        public static Point3D operator *(double t, Point3D point)
        {
            return Multiply(point, t);
        }
        //
        // Summary:
        //     Multiplies a Point3D by a number.
        //
        // Parameters:
        //   point:
        //     A point.
        //
        //   t:
        //     A number.
        //
        // Returns:
        //     A new point that is coordinatewise multiplied by t.
        public static Point3D operator *(Point3D point, double t)
        {
            return Multiply(point, t);
        }
        //
        // Summary:
        //     Divides a Point3D by a number.
        //
        // Parameters:
        //   point:
        //     A point.
        //
        //   t:
        //     A number.
        //
        // Returns:
        //     A new point that is coordinatewise divided by t.
        public static Point3D operator /(Point3D point, double t)
        {
            return Divide(point, t);
        }
        //
        // Summary:
        //     Determines whether two Point3D have equal values.
        //
        // Parameters:
        //   a:
        //     The first point.
        //
        //   b:
        //     The second point.
        //
        // Returns:
        //     true if the coordinates of the two points are exactly equal; otherwise false.
        public static bool operator ==(Point3D a, Point3D b)
        {
            return a.Equals(b);
        }
        //
        // Summary:
        //     Determines whether two Point3D have different values.
        //
        // Parameters:
        //   a:
        //     The first point.
        //
        //   b:
        //     The second point.
        //
        // Returns:
        //     true if the two points differ in any coordinate; false otherwise.
        public static bool operator !=(Point3D a, Point3D b)
        {
            return !a.Equals(b);
        }
        //
        // Summary:
        //     Determines whether the first specified point comes before (has inferior sorting
        //     value than) the second point.
        //     Coordinates evaluation priority is first X, then Y, then Z.
        //
        // Parameters:
        //   a:
        //     The first point.
        //
        //   b:
        //     The second point.
        //
        // Returns:
        //     true if a.X is smaller than b.X, or a.X == b.X and a.Y is smaller than b.Y, or
        //     a.X == b.X and a.Y == b.Y and a.Z is smaller than b.Z; otherwise, false.
        public static bool operator <(Point3D a, Point3D b)
        {
            return -1 == a.CompareTo(b);
        }
        //
        // Summary:
        //     Determines whether the first specified point comes after (has superior sorting
        //     value than) the second point.
        //     Coordinates evaluation priority is first X, then Y, then Z.
        //
        // Parameters:
        //   a:
        //     The first point.
        //
        //   b:
        //     The second point.
        //
        // Returns:
        //     true if a.X is larger than b.X, or a.X == b.X and a.Y is larger than b.Y, or
        //     a.X == b.X and a.Y == b.Y and a.Z is larger than b.Z; otherwise, false.
        public static bool operator >(Point3D a, Point3D b)
        {
            return 1 == a.CompareTo(b);
        }
        //
        // Summary:
        //     Determines whether the first specified point comes before (has inferior sorting
        //     value than) the second point, or it is equal to it.
        //     Coordinates evaluation priority is first X, then Y, then Z.
        //
        // Parameters:
        //   a:
        //     The first point.
        //
        //   b:
        //     The second point.
        //
        // Returns:
        //     true if a.X is smaller than b.X, or a.X == b.X and a.Y is smaller than b.Y, or
        //     a.X == b.X and a.Y == b.Y and a.Z <= b.Z; otherwise, false.
        public static bool operator <=(Point3D a, Point3D b)
        {
            return 1 != a.CompareTo(b);
        }
        //
        // Summary:
        //     Determines whether the first specified point comes after (has superior sorting
        //     value than) the second point, or it is equal to it.
        //     Coordinates evaluation priority is first X, then Y, then Z.
        //
        // Parameters:
        //   a:
        //     The first point.
        //
        //   b:
        //     The second point.
        //
        // Returns:
        //     true if a.X is larger than b.X, or a.X == b.X and a.Y is larger than b.Y, or
        //     a.X == b.X and a.Y == b.Y and a.Z >= b.Z; otherwise, false.
        public static bool operator >=(Point3D a, Point3D b)
        {
            return 1 != a.CompareTo(b);
        }
        //
        // Summary:
        //     Converts a point in a control point, without needing casting.
        //
        // Parameters:
        //   pt:
        //     The point.
        //
        // Returns:
        //     The control point.
        //**** public static implicit operator ControlPoint(Point3D pt);
        //
        // Summary:
        //     Converts a single-precision point in a double-precision point, without needing
        //     casting.
        //
        // Parameters:
        //   point:
        //     A point.
        //
        // Returns:
        //     The resulting point.
        //**** public static implicit operator Point3D(Point3f point);
        //
        // Summary:
        //     Converts a point in a vector, needing casting.
        //
        // Parameters:
        //   point:
        //     A point.
        //
        // Returns:
        //     The resulting vector.
        public static explicit operator Vector3D(Point3D point)
        {
            return new Vector3D(point.X, point.Y, point.Z);
        }
        //
        // Summary:
        //     Converts a vector in a point, needing casting.
        //
        // Parameters:
        //   vector:
        //     A vector.
        //
        // Returns:
        //     The resulting point.
        public static explicit operator Point3D(Vector3D vector)
        {
            return new Point3D(vector.X, vector.Y, vector.Z);
        }

        public static Point3D NegativeInfinity
        {
            get { return new Point3D(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity); }
        }

        public static Point3D PositiveInfinity
        {
            get { return new Point3D(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity); }
        }

        public static Box3D Bounds(Point3D[] points, int count)
        {
            Point3D min = new Point3D(points[0]);
            Point3D max = new Point3D(points[0]);

            for (int i = 0; i < count; i++)
            {
                Point3D point = points[i];

                if (point.X < min.X) { min.X = point.X; }
                else if (point.X > max.X) { max.X = point.X; }
                if (point.Y < min.Y) { min.Y = point.Y; }
                else if (point.Y > max.Y) { max.Y = point.Y; }
                if (point.Z < min.Z) { min.Z = point.Z; }
                else if (point.Z > max.Z) { max.Z = point.Z; }
            }

            return new Box3D { Point1 = min, Point2 = max };
        }

        public void Set(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }

        public void Set(Point3D point)
        {
            Set(point.X, point.Y, point.Z);
        }

        public void Zero()
        {
            Set(0.0, 0.0, 0.0);
        }

        public bool IsZero
        {
            get { return 0 == X && 0 == Y && 0 == Z; }
        }

        object ICloneable.Clone()
        {
            return new Point3D(this);
        }

        int IComparable.CompareTo(object obj)
        {
            return (obj is Point3D point) ? CompareTo(point) : -1;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        public void SwapXY()
        {
            double temp = X;
            X = Y; Y = temp;
        }

        public void SwapYZ()
        {
            double temp = Y;
            Y = Z; Z = temp;
        }

        public void SwapZX()
        {
            double temp = Z;
            Z = X; X = temp;
        }

        public static Point3D Multiply(Point3D point, Vector3D vector)
        {
            return new Point3D(point.X * vector.X, point.Y * vector.Y, point.Z * vector.Z);
        }

        public static Point3D operator *(Point3D point, Vector3D vector)
        {
            return Multiply(point, vector);
        }

        public static Point3D operator *(Vector3D vector, Point3D point)
        {
            return Multiply(point, vector);
        }

        public void Scale(double scaleX, double scaleY, double scaleZ)
        {
            X *= scaleX;
            Y *= scaleY;
            Z *= scaleZ;
        }


        public void Translate(double deltaX, double deltaY, double deltaZ)
        {
            X += deltaX;
            Y += deltaY;
            Z += deltaZ;
        }

        public static void Rotate(double angle, ref double coord1, ref double coord2)
        {
            double radians = angle * Math.PI / 180;
            double cosine = Math.Cos(radians);
            double sine = Math.Sin(radians);
            double c1 = coord1 * cosine - coord2 * sine;
            double c2 = coord1 * sine + coord2 * cosine;
            coord1 = c1; coord2 = c2;
        }

        public void RotateX(double angle)
        {
            double y = Y, z = Z;
            Rotate(angle, ref y, ref z);
            Y = y; Z = z;
        }

        public void RotateY(double angle)
        {
            double z = Z, x = X;
            Rotate(angle, ref z, ref x);
            Z = z; X = x;
        }

        public void RotateZ(double angle)
        {
            double x = X, y = Y;
            Rotate(angle, ref x, ref y);
            X = x; Y = y;
        }

        public void Project(Transform3D xform)
        {
            double x, y, z, w;
            x = xform.M00 * X + xform.M01 * Y + xform.M02 * Z + xform.M03;
            y = xform.M10 * X + xform.M11 * Y + xform.M12 * Z + xform.M13;
            z = xform.M20 * X + xform.M21 * Y + xform.M22 * Z + xform.M23;
            w = xform.M30 * X + xform.M31 * Y + xform.M32 * Z + xform.M33;

            if (0.0 == w) { w = 1.0; }

            X = x / w;
            Y = y / w;
            Z = z / w;

            Clamp(-4096, 4096);
        }

        public void Project(bool ortho, double width, double height, double fov, double distance)
        {
            double divisor = (distance - this.Z);
            double factor = (ortho || 0 == divisor) ? fov / distance : fov / divisor;
            X = this.X * factor + (width * 0.5);
            Y = -this.Y * factor + (height * 0.5);

            Clamp(-4096, 4096);
        }

        private void Clamp(double min, double max)
        {
            X = Clamp(X, min, max);
            Y = Clamp(Y, min, max);
            Z = Clamp(Z, min, max);
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value <= min) { return min; }
            if (value >= max) { return max; }
            return value;
        }
    }
}
