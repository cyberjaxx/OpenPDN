using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

namespace Cyberjax.Geometry
{
    //
    // Summary:
    //     Represents the three components of a vector in three-dimensional space, using
    //     System.Double-precision floating point numbers.
    [DebuggerDisplay("({X}, {Y}, {Z})")]

    public struct Vector3D : ISerializable, IEquatable<Vector3D>, IComparable<Vector3D>, IComparable, IEpsilonComparable<Vector3D>, ICloneable
    {
        //
        // Summary:
        //     Initializes a new instance of a vector, copying the three components from the
        //     three coordinates of a point.
        //
        // Parameters:
        //   point:
        //     The point to copy from.
        public Vector3D(Point3D point)
        {
            X = point.X;
            Y = point.Y;
            Z = point.Z;
        }
        //
        // Summary:
        //     Initializes a new instance of a vector, copying the three components from a single-precision
        //     vector.
        //
        // Parameters:
        //   vector:
        //     A single-precision vector.
        //**** public Vector3D(Vector3f vector);
        //
        // Summary:
        //     Initializes a new instance of a vector, copying the three components from a vector.
        //
        // Parameters:
        //   vector:
        //     A double-precision vector.
        public Vector3D(Vector3D vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }
        //
        // Summary:
        //     Initializes a new instance of a vector, using its three components.
        //
        // Parameters:
        //   x:
        //     The X (first) component.
        //
        //   y:
        //     The Y (second) component.
        //
        //   z:
        //     The Z (third) component.
        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        //
        // Summary:
        //     Gets or sets a vector component at the given index.
        //
        // Parameters:
        //   index:
        //     Index of vector component. Valid values are:
        //     0 = X-component
        //     1 = Y-component
        //     2 = Z-component
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
        //     Gets the value of the vector with components 0,0,0.
        public static Vector3D Zero
        {
            get { return new Vector3D(0.0, 0.0, 0.0); }
        }
        //
        // Summary:
        //     Gets the value of the vector with each component set to UnsetValue.
        public static Vector3D Unset
        {
            get { return new Vector3D(Point3D.UnsetValue, Point3D.UnsetValue, Point3D.UnsetValue); }
        }
        //
        // Summary:
        //     Gets the value of the vector with components 0,0,1.
        public static Vector3D ZAxis
        {
            get { return new Vector3D(0.0, 0.0, 1.0); }
        }
        //
        // Summary:
        //     Gets the value of the vector with components 0,1,0.
        public static Vector3D YAxis
        {
            get { return new Vector3D(0.0, 1.0, 0.0); }
        }
        //
        // Summary:
        //     Gets the value of the vector with components 1,0,0.
        public static Vector3D XAxis
        {
            get { return new Vector3D(1.0, 0.0, 0.0); }
        }
        //
        // Summary:
        //     Computes the length (or magnitude, or size) of this vector. This is an application
        //     of Pythagoras' theorem. If this vector is invalid, its length is considered 0.
        public double Length
        {
            get { return Math.Sqrt(X * X + Y * Y + Z * Z); }
        }
        //
        // Summary:
        //     Gets the largest (both positive and negative) component value in this vector.
        public double MaximumCoordinate
        {
            get { return Math.Abs(X) > Math.Abs(Y) ?
                    (Math.Abs(Z) > Math.Abs(X) ? Z : X) :
                    (Math.Abs(Z) > Math.Abs(Y) ? Z : Y); }
        }
        //
        // Summary:
        //     Gets the smallest (both positive and negative) component value in this vector.
        public double MinimumCoordinate
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
        //     Gets a value indicating whether this vector is valid. A valid vector must be
        //     formed of valid component values for x, y and z.
        public bool IsValid
        {
            get { return !double.IsNaN(X) && !double.IsNaN(Y) && !double.IsNaN(Z); }
        }
        //
        // Summary:
        //     Gets or sets the Z (third) component of the vector.
        public double Z { get; set; }
        //
        // Summary:
        //     Gets or sets the Y (second) component of the vector.
        public double Y { get; set; }
        //
        // Summary:
        //     Gets or sets the X (first) component of the vector.
        public double X { get; set; }
        //
        // Summary:
        //     Computes the squared length (or magnitude, or size) of this vector. This is an
        //     application of Pythagoras' theorem. While the Length property checks for input
        //     validity, this property does not. You should check validity in advance, if this
        //     vector can be invalid.
        public double SquareLength
        {
            get { return X * X + Y * Y + Z * Z; }
        }
        //
        // Summary:
        //     Gets a value indicating whether or not this is a unit vector. A unit vector has
        //     length 1.
        public bool IsUnitVector
        {
            // check SquareLength and avoid the Math.Sqrt
            get { return 1.0 == SquareLength; }
        }
        //
        // Summary:
        //     Gets a value indicating whether the X, Y, and Z values are all equal to 0.0.
        public bool IsZero
        {
            get { return 0.0 == X && 0.0 == Y && 0.0 == Z; }
        }

        //
        // Summary:
        //     Sums up two vectors.
        //     (Provided for languages that do not support operator overloading. You can use
        //     the + operator otherwise)
        //
        // Parameters:
        //   vector1:
        //     A vector.
        //
        //   vector2:
        //     A second vector.
        //
        // Returns:
        //     A new vector that results from the componentwise addition of the two vectors.
        public static Vector3D Add(Vector3D vector1, Vector3D vector2)
        {
            return new Vector3D(vector1.X + vector2.X, vector1.Y + vector2.Y, vector1.Z + vector2.Z);
        }
        //
        // Summary:
        //     Computes the cross product (aka "vector product", aka "exterior product") of two vectors.
        //     This operation is not commutative.
        //
        // Parameters:
        //   a:
        //     First vector.
        //
        //   b:
        //     Second vector.
        //
        // Returns:
        //     A new vector that is perpendicular to both a and b,
        //     has Length == a.Length * b.Length and
        //     with a result that is oriented following the right hand rule.
        public static Vector3D CrossProduct(Vector3D a, Vector3D b)
        {
            double x = a.Y * b.Z - b.Y * a.Z;
            double y = a.Z * b.X - b.Z * a.X;
            double z = a.X * b.Y - b.X * a.Y;
            return new Vector3D(x, y, z);
        }
        //
        // Summary:
        //     Divides a Rhino.Geometry.Vector3D by a number, having the effect of shrinking
        //     it.
        //     (Provided for languages that do not support operator overloading. You can use
        //     the / operator otherwise)
        //
        // Parameters:
        //   vector:
        //     A vector.
        //
        //   t:
        //     A number.
        //
        // Returns:
        //     A new vector that is componentwise divided by t.
        public static Vector3D Divide(Vector3D vector, double t)
        {
            return new Vector3D(vector.X / t, vector.Y / t, vector.Z / t);
        }
        //
        // Summary:
        //     Multiplies a vector by a number, having the effect of scaling it.
        //     (Provided for languages that do not support operator overloading. You can use
        //     the * operator otherwise)
        //
        // Parameters:
        //   t:
        //     A number.
        //
        //   vector:
        //     A vector.
        //
        // Returns:
        //     A new vector that is the original vector coordinatewise multiplied by t.
        public static Vector3D Multiply(double t, Vector3D vector)
        {
            return Multiply(vector, t);
        }
        //
        // Summary:
        //     Multiplies two vectors together, returning the dot product (or inner product).
        //     This differs from the cross product.
        //     (Provided for languages that do not support operator overloading. You can use
        //     the * operator otherwise)
        //
        // Parameters:
        //   vector1:
        //     A vector.
        //
        //   vector2:
        //     A second vector.
        //
        // Returns:
        //     A value that results from the evaluation of v1.X*v2.X + v1.Y*v2.Y + v1.Z*v2.Z.
        //     This value equals v1.Length * v2.Length * cos(alpha), where alpha is the angle
        //     between vectors.
        public static double Multiply(Vector3D vector1, Vector3D vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
        }
        //
        // Summary:
        //     Multiplies a vector by a number, having the effect of scaling it.
        //     (Provided for languages that do not support operator overloading. You can use
        //     the * operator otherwise)
        //
        // Parameters:
        //   vector:
        //     A vector.
        //
        //   t:
        //     A number.
        //
        // Returns:
        //     A new vector that is the original vector coordinatewise multiplied by t.
        public static Vector3D Multiply(Vector3D vector, double t)
        {
            return new Vector3D(vector.X * t, vector.Y * t, vector.Z * t);
        }
        //
        // Summary:
        //     Computes the reversed vector.
        //     (Provided for languages that do not support operator overloading. You can use
        //     the - unary operator otherwise)
        //
        // Parameters:
        //   vector:
        //     A vector to negate.
        //
        // Returns:
        //     A new vector where all components were multiplied by -1.
        //
        // Remarks:
        //     Similar to Rhino.Geometry.Vector3D.Reverse, but static for CLR compliance.
        public static Vector3D Negate(Vector3D vector)
        {
            return new Vector3D(-vector.X, -vector.Y, -vector.Z);
        }
        //
        // Summary:
        //     Subtracts the second vector from the first one.
        //     (Provided for languages that do not support operator overloading. You can use
        //     the - operator otherwise)
        //
        // Parameters:
        //   vector1:
        //     A vector.
        //
        //   vector2:
        //     A second vector.
        //
        // Returns:
        //     A new vector that results from the componentwise difference of vector1 - vector2.
        public static Vector3D Subtract(Vector3D vector1, Vector3D vector2)
        {
            return new Vector3D(vector1.X - vector2.X, vector1.Y - vector2.Y, vector1.Z - vector2.Z);
        }
        //
        // Summary:
        //     Computes the angle of v1, v2 with a normal vector.
        //
        //     A || N = N × (A × N) / | N |²
        //     B || N = N x (A x N) / | N |²
        //     ϴ = ((A || N) · (B || N)) / (| A || N | * | B || N |)
        //
        // Parameters:
        //   v1:
        //     First vector.
        //
        //   v2:
        //     Second vector.
        //
        //   vNormal:
        //     Normal vector.
        //
        // Returns:
        //     On success, the angle (in radians) between a and b with respect of normal vector;
        //     UnsetValue on failure.
        //
        // A || N = N × (A × N) / | N |²
        // B || N = N x (A x N) / | N |²
        // ϴ = arcos((AN · BN) / (| AN | * | BN |))
        // since we don't care about the magnitude of the vectors
        // we can simplify the equation to
        // ϴ = arcos((N×A×N · N×B×N) / (| N×A×N | * | N×B×N |))
        //
        public static double VectorAngle(Vector3D v1, Vector3D v2, Vector3D vNormal)
        {
            Vector3D NAN = CrossProduct(vNormal, CrossProduct(v1, vNormal));
            Vector3D NBN = CrossProduct(vNormal, CrossProduct(v2, vNormal));
            return VectorAngle(NAN, NBN);
        }
        //
        // Summary:
        //     Computes the angle on a plane between two vectors.
        //
        // Parameters:
        //   a:
        //     First vector.
        //
        //   b:
        //     Second vector.
        //
        //   plane:
        //     Two-dimensional plane on which to perform the angle measurement.
        //
        // Returns:
        //     On success, the angle (in radians) between a and b as projected onto the plane;
        //     UnsetValue on failure.
        public static double VectorAngle(Vector3D a, Vector3D b, Plane3D plane)
        {
            return VectorAngle(a, b, plane.Normal);
        }
        //
        // Summary:
        //     Compute the angle between two vectors.
        //     This operation is commutative.
        //
        // Parameters:
        //   a:
        //     First vector for angle.
        //
        //   b:
        //     Second vector for angle.
        //
        // Returns:
        //     If the input is valid, the angle (in radians) between a and b; UnsetValue
        //     otherwise.
        public static double VectorAngle(Vector3D a, Vector3D b)
        {
            double denominator = a.Length * b.Length;
            return 0 == denominator ? Point3D.UnsetValue : Math.Acos((a * b) / denominator);
        }
        //
        // Summary:
        //     Compares this Rhino.Geometry.Vector3D with another Rhino.Geometry.Vector3D.
        //     Component evaluation priority is first X, then Y, then Z.
        //
        // Parameters:
        //   other:
        //     The other Rhino.Geometry.Vector3D to use in comparison.
        //
        // Returns:
        //     0: if this is identical to other
        //     -1: if X < other.X
        //     -1: if X == other.X and Y < other.Y
        //     -1: if X == other.X and Y == other.Y and Z < other.Z
        //     +1: otherwise.
        public int CompareTo(Vector3D other)
        {
            if (X < other.X) { return -1; }
            if (X > other.X) { return 1; }
            if (Y < other.Y) { return -1; }
            if (Y > other.Y) { return 1; }
            if (Z < other.Z) { return -1; }
            if (Z > other.Z) { return 1; }
            return 0;
        }
        //
        // Summary:
        //     Check that all values in other are within epsilon of the values in this
        //
        // Parameters:
        //   other:
        //
        //   epsilon:
        public bool EpsilonEquals(Vector3D other, double epsilon)
        {
            double dX = X - other.X;
            if (dX < -epsilon || dX > epsilon) { return false; }
            double dY = Y - other.Y;
            if (dY < -epsilon || dY > epsilon) { return false; }
            double dZ = Z - other.Z;
            if (dZ < -epsilon || dZ > epsilon) { return false; }
            return true;
        }
        //
        // Summary:
        //     Determines whether the specified System.Object is a Vector3D and has the same
        //     values as the present vector.
        //
        // Parameters:
        //   obj:
        //     The specified object.
        //
        // Returns:
        //     true if obj is a Vector3D and has the same coordinates as this; otherwise false.
        public override bool Equals(object obj)
        {
            return (obj is Vector3D vector) ? Equals(vector) : false;
        }
        //
        // Summary:
        //     Determines whether the specified vector has the same value as the present vector.
        //
        // Parameters:
        //   vector:
        //     The specified vector.
        //
        // Returns:
        //     true if vector has the same coordinates as this; otherwise false.
        public bool Equals(Vector3D vector)
        {
            return X == vector.X && Y == vector.Y && Z == vector.Z;
        }
        //
        // Summary:
        //     Computes the hash code for the current vector.
        //
        // Returns:
        //     A non-unique number that represents the components of this vector.
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }
        //
        // Summary:
        //     Determines whether this vector is parallel to another vector, within a provided
        //     tolerance.
        //
        // Parameters:
        //   other:
        //     Vector to use for comparison.
        //
        //   angleTolerance:
        //     Angle tolerance (in radians).
        //
        // Returns:
        //     Parallel indicator:
        //     +1 = both vectors are parallel.
        //     0 = vectors are not parallel or at least one of the vectors is zero.
        //     -1 = vectors are anti-parallel.
        public int IsParallelTo(Vector3D other, double angleTolerance)
        {
            double theta = VectorAngle(this, other);
            if (theta >= 0)
            {
                if (theta < angleTolerance) { return 1; }
                if (theta > Math.PI - angleTolerance) { return -1; }
            }
            else
            {
                if (theta > -angleTolerance) { return 1; }
                if (theta < -Math.PI + angleTolerance) { return -1; }
            }
            return 0;
        }
        //
        // Summary:
        //     Determines whether this vector is parallel to another vector, within one degree
        //     (within Pi / 180).
        //
        // Parameters:
        //   other:
        //     Vector to use for comparison.
        //
        // Returns:
        //     Parallel indicator:
        //     +1 = both vectors are parallel
        //     0 = vectors are not parallel, or at least one of the vectors is zero
        //     -1 = vectors are anti-parallel.
        public int IsParallelTo(Vector3D other)
        {
            double oneDegree = Math.PI / 180.0;
            return IsParallelTo(other, oneDegree);
        }
        //
        // Summary:
        //     Determines whether this vector is perpendicular to another vector, within a provided
        //     angle tolerance.
        //
        // Parameters:
        //   other:
        //     Vector to use for comparison.
        //
        //   angleTolerance:
        //     Angle tolerance (in radians).
        //
        // Returns:
        //     true if vectors form Pi-radians (90-degree) angles with each other; otherwise
        //     false.
        public bool IsPerpendicularTo(Vector3D other, double angleTolerance)
        {
            double theta = VectorAngle(this, other);
            if (theta > 0)
            {
                theta -= Math.PI / 2.0;
            }
            else
            {
                theta += Math.PI / 2.0;
            }

            return (theta > -angleTolerance) && (theta < angleTolerance);
        }
        //
        // Summary:
        //     Test to see whether this vector is perpendicular to within one degree of another
        //     one.
        //
        // Parameters:
        //   other:
        //     Vector to compare to.
        //
        // Returns:
        //     true if both vectors are perpendicular, false if otherwise.
        public bool IsPerpendicularTo(Vector3D other)
        {
            return IsPerpendicularTo(other, Math.PI / 180.0);
        }
        //
        // Summary:
        //     Uses ZeroTolerance for IsTiny calculation.
        //
        // Returns:
        //     true if vector is very small, otherwise false.
        public bool IsTiny()
        {
            return IsTiny(Point3D.ZeroTolerance);
        }
        //
        // Summary:
        //     Determines whether a vector is very short.
        //
        // Parameters:
        //   tolerance:
        //     A nonzero value used as the coordinate zero tolerance. .
        //
        // Returns:
        //     (Math.Abs(X) <= tiny_tol) AND (Math.Abs(Y) <= tiny_tol) AND (Math.Abs(Z) <= tiny_tol).
        public bool IsTiny(double tolerance)
        {
            return (Math.Abs(X) <= tolerance) && (Math.Abs(Y) <= tolerance) && (Math.Abs(Z) <= tolerance);
        }
        //
        // Summary:
        //     Sets this vector to be perpendicular to another vector. Result is not unitized.
        //
        // Parameters:
        //   other:
        //     Vector to use as guide.
        //
        // Returns:
        //     true on success, false if input vector is zero or invalid.
        public bool PerpendicularTo(Vector3D other)
        {
            if (!IsValid || IsZero) { return false; }

            double temp = X;
            if (0.0 == Y)
            {
                X = Z;
                Z = temp;
            }
            else
            {
                X = Y;
                Y = temp;
                Z = 0.0;
            }
            return true;
        }
        //
        // Summary:
        //     Reverses this vector in place (reverses the direction).
        //     If this vector is Invalid, no changes will occur and false will be returned.
        //
        // Returns:
        //     true on success or false if the vector is invalid.
        //
        // Remarks:
        //     Similar to Rhino.Geometry.Vector3D.Negate(Rhino.Geometry.Vector3D), that is only
        //     provided for CLR language compliance.
        public bool Reverse()
        {
            if (!IsValid) { return false; }

            X = -X; Y = -Y; Z = -Z;
            return true;
        }
        //
        // Summary:
        //     Rotates this vector around a given axis.
        //
        // Parameters:
        //   angleRadians:
        //     Angle of rotation (in radians).
        //
        //   rotationAxis:
        //     Axis of rotation.
        //
        // Returns:
        //     true on success, false on failure.
        public bool Rotate(double angleRadians, Vector3D rotationAxis)
        {
            if (!rotationAxis.IsValid || rotationAxis.IsZero) { return false; }

            Transform(Transform3D.Rotation(angleRadians, rotationAxis, Point3D.Origin));
            return true;
        }
        //
        // Summary:
        //     Returns the string representation of the current vector, in the form X,Y,Z.
        //
        // Returns:
        //     A string with the current location of the point.
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", X, Y, Z);
        }
        //
        // Summary:
        //     Transforms the vector in place.
        //     The transformation matrix acts on the left of the vector; i.e.,
        //     result = transformation*vector
        //
        // Parameters:
        //   transformation:
        //     Transformation matrix to apply.
        public void Transform(Transform3D transformation)
        {
            double x = X, y = Y, z = Z;
            X = transformation.M00 * x + transformation.M01 * y + transformation.M02 * z + transformation.M03;
            Y = transformation.M10 * x + transformation.M11 * y + transformation.M12 * z + transformation.M13;
            Z = transformation.M20 * x + transformation.M21 * y + transformation.M22 * z + transformation.M23;
        }
        //
        // Summary:
        //     Unitizes the vector in place. A unit vector has length 1 unit.
        //     An invalid or zero length vector cannot be unitized.
        //
        // Returns:
        //     true on success or false on failure.
        public bool Unitize()
        {
            if (!IsValid || IsZero) { return false; }

            double length = Length;
            X /= length;
            Y /= length;
            Z /= length;
            return true;
        }
        //
        // Summary:
        //     Sums up two vectors.
        //
        // Parameters:
        //   vector1:
        //     A vector.
        //
        //   vector2:
        //     A second vector.
        //
        // Returns:
        //     A new vector that results from the componentwise addition of the two vectors.
        public static Vector3D operator +(Vector3D vector1, Vector3D vector2)
        {
            return Add(vector1, vector2);
        }
        //
        // Summary:
        //     Computes the opposite vector.
        //
        // Parameters:
        //   vector:
        //     A vector to negate.
        //
        // Returns:
        //     A new vector where all components were multiplied by -1.
        public static Vector3D operator -(Vector3D vector)
        {
            return Negate(vector);
        }
        //
        // Summary:
        //     Subtracts the second vector from the first one.
        //
        // Parameters:
        //   vector1:
        //     A vector.
        //
        //   vector2:
        //     A second vector.
        //
        // Returns:
        //     A new vector that results from the componentwise difference of vector1 - vector2.
        public static Vector3D operator -(Vector3D vector1, Vector3D vector2)
        {
            return Subtract(vector1, vector2);
        }
        //
        // Summary:
        //     Multiplies two vectors together, returning the dot product (or inner product).
        //     This differs from the cross product.
        //
        // Parameters:
        //   vector1:
        //     A vector.
        //
        //   vector2:
        //     A second vector.
        //
        // Returns:
        //     A value that results from the evaluation of v1.X*v2.X + v1.Y*v2.Y + v1.Z*v2.Z.
        //     This value equals v1.Length * v2.Length * cos(alpha), where alpha is the angle
        //     between vectors.
        public static double operator *(Vector3D vector1, Vector3D vector2)
        {
            return Multiply(vector1, vector2);
        }
        //
        // Summary:
        //     Multiplies a vector by a number, having the effect of scaling it.
        //
        // Parameters:
        //   t:
        //     A number.
        //
        //   vector:
        //     A vector.
        //
        // Returns:
        //     A new vector that is the original vector coordinatewise multiplied by t.
        public static Vector3D operator *(double t, Vector3D vector)
        {
            return Multiply(vector, t);
        }
        //
        // Summary:
        //     Multiplies a vector by a number, having the effect of scaling it.
        //
        // Parameters:
        //   vector:
        //     A vector.
        //
        //   t:
        //     A number.
        //
        // Returns:
        //     A new vector that is the original vector coordinatewise multiplied by t.
        public static Vector3D operator *(Vector3D vector, double t)
        {
            return Multiply(vector, t);
        }
        //
        // Summary:
        //     Divides a Rhino.Geometry.Vector3D by a number, having the effect of shrinking
        //     it.
        //
        // Parameters:
        //   vector:
        //     A vector.
        //
        //   t:
        //     A number.
        //
        // Returns:
        //     A new vector that is componentwise divided by t.
        public static Vector3D operator /(Vector3D vector, double t)
        {
            return Divide(vector, t);
        }
        //
        // Summary:
        //     Determines whether two vectors have the same value.
        //
        // Parameters:
        //   a:
        //     A vector.
        //
        //   b:
        //     Another vector.
        //
        // Returns:
        //     true if all coordinates are pairwise equal; false otherwise.
        public static bool operator ==(Vector3D a, Vector3D b)
        {
            return a.Equals(b);
        }
        //
        // Summary:
        //     Determines whether two vectors have different values.
        //
        // Parameters:
        //   a:
        //     A vector.
        //
        //   b:
        //     Another vector.
        //
        // Returns:
        //     true if any coordinate pair is different; false otherwise.
        public static bool operator !=(Vector3D a, Vector3D b)
        {
            return !a.Equals(b);
        }
        //
        // Summary:
        //     Determines whether the first specified vector comes before (has inferior sorting
        //     value than) the second vector.
        //     Components evaluation priority is first X, then Y, then Z.
        //
        // Parameters:
        //   a:
        //     The first vector.
        //
        //   b:
        //     The second vector.
        //
        // Returns:
        //     true if a.X is smaller than b.X, or a.X == b.X and a.Y is smaller than b.Y, or
        //     a.X == b.X and a.Y == b.Y and a.Z is smaller than b.Z; otherwise, false.
        public static bool operator <(Vector3D a, Vector3D b)
        {
            return -1 == a.CompareTo(b);
        }
        //
        // Summary:
        //     Determines whether the first specified vector comes after (has superior sorting
        //     value than) the second vector.
        //     Components evaluation priority is first X, then Y, then Z.
        //
        // Parameters:
        //   a:
        //     The first vector.
        //
        //   b:
        //     The second vector.
        //
        // Returns:
        //     true if a.X is larger than b.X, or a.X == b.X and a.Y is larger than b.Y, or
        //     a.X == b.X and a.Y == b.Y and a.Z is larger than b.Z; otherwise, false.
        public static bool operator >(Vector3D a, Vector3D b)
        {
            return 1 == a.CompareTo(b);
        }
        //
        // Summary:
        //     Determines whether the first specified vector comes before (has inferior sorting
        //     value than) the second vector, or it is equal to it.
        //     Components evaluation priority is first X, then Y, then Z.
        //
        // Parameters:
        //   a:
        //     The first vector.
        //
        //   b:
        //     The second vector.
        //
        // Returns:
        //     true if a.X is smaller than b.X, or a.X == b.X and a.Y is smaller than b.Y, or
        //     a.X == b.X and a.Y == b.Y and a.Z <= b.Z; otherwise, false.
        public static bool operator <=(Vector3D a, Vector3D b)
        {
            return 1 != a.CompareTo(b);
        }
        //
        // Summary:
        //     Determines whether the first specified vector comes after (has superior sorting
        //     value than) the second vector, or it is equal to it.
        //     Components evaluation priority is first X, then Y, then Z.
        //
        // Parameters:
        //   a:
        //     The first vector.
        //
        //   b:
        //     The second vector.
        //
        // Returns:
        //     true if a.X is larger than b.X, or a.X == b.X and a.Y is larger than b.Y, or
        //     a.X == b.X and a.Y == b.Y and a.Z >= b.Z; otherwise, false.
        public static bool operator >=(Vector3D a, Vector3D b)
        {
            return -1 != a.CompareTo(b);
        }

        //
        // Summary:
        //     Converts a single-precision (float) vector in a double-precision vector, without
        //     needing casting.
        //
        // Parameters:
        //   vector:
        //     A single-precision vector.
        //
        // Returns:
        //     The same vector, expressed using double-precision values.
        //**** public static implicit operator Vector3D(Vector3f vector);


        public void Set(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }

        public void Set(Vector3D vector)
        {
            Set(vector.X, vector.Y, vector.Z);
        }

        public void Scale(double scaleX, double scaleY, double scaleZ)
        {
            X *= scaleX;
            Y *= scaleY;
            Z *= scaleZ;
        }

        public Vector3D SwapXY()
        {
            return new Vector3D(Y, X, Z);
        }

        public Vector3D SwapYZ()
        {
            return new Vector3D(X, Z, Y);
        }

        public Vector3D SwapZX()
        {
            return new Vector3D(Z, Y, X);
        }

        int IComparable.CompareTo(object obj)
        {
            return (obj is Vector3D vector) ? CompareTo(vector) : -1;
        }

        object ICloneable.Clone()
        {
            return new Vector3D(this);
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
