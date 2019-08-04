#region Assembly RhinoCommon, Version=6.0.17241.13071, Culture=neutral, PublicKeyToken=552281e97c755530
// C:\Program Files\Rhino WIP\System\RhinoCommon.dll
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cyberjax.Geometry
{
    //
    // Summary:
    //     Represents the values in a 4x4 transform matrix.
    //     This is parallel to C++ ON_Xform.
    //    [DefaultMember("Item")]
    public struct Transform3D : IComparable<Transform3D>, IComparable, IEquatable<Transform3D>, ICloneable
    {
        public const double UnsetValue = -1.23432101234321E+308;
        private const int ROWS = 4, COLS = 4;
        public double[,] Matrix;

        //
        // Summary:
        //     Initializes a new transform matrix with a specified value along the diagonal.
        //
        // Parameters:
        //   diagonalValue:
        //     Value to assign to all diagonal cells except M33 which is set to 1.0.
        public Transform3D(double diagonalValue)
        {
            Matrix = new double[ROWS, COLS]
            {
                { diagonalValue, 0.0, 0.0, 0.0 },
                { 0.0, diagonalValue, 0.0, 0.0 },
                { 0.0, 0.0, diagonalValue, 0.0 },
                { 0.0, 0.0, 0.0, 1.0 }
            };
        }
        //
        // Summary:
        //     Initializes a new transform matrix with a specified value.
        //
        // Parameters:
        //   value:
        //     Value to assign to all cells.
        public Transform3D(Transform3D value)
        {
            Matrix = new double[ROWS, COLS]
            {
                { value.M00, value.M01, value.M02, value.M03 },
                { value.M10, value.M11, value.M12, value.M13 },
                { value.M20, value.M21, value.M22, value.M23 },
                { value.M30, value.M31, value.M32, value.M33 }
            };
        }
        //
        // Summary:
        //     Gets or sets the matrix value at the given row and column indixes.
        //
        // Parameters:
        //   row:
        //     Index of row to access, must be 0, 1, 2 or 3.
        //
        //   column:
        //     Index of column to access, must be 0, 1, 2 or 3.
        //
        // Returns:
        //     The value at [row, column]
        public double this[int row, int column]
        {
            get { return Matrix[row, column]; }
            set { Matrix[row, column] = value; }
        }

        //
        // Summary:
        //     Gets an XForm filled with RhinoMath.UnsetValue.
        public static Transform3D Unset
        {
            get
            {
                return new Transform3D
                {
                    Matrix = new double[ROWS, COLS]
                    {
                        { UnsetValue, UnsetValue, UnsetValue, UnsetValue },
                        { UnsetValue, UnsetValue, UnsetValue, UnsetValue },
                        { UnsetValue, UnsetValue, UnsetValue, UnsetValue },
                        { UnsetValue, UnsetValue, UnsetValue, UnsetValue }
                    }
                };
            }
        }
        //
        // Summary:
        //     Gets a new identity transform matrix. An identity matrix defines no transformation.
        public static Transform3D Identity
        {
            get { return new Transform3D(1); }
        }
        //
        // Summary:
        //     Gets or sets this[0,0].
        public double M00 { get { return this[0, 0]; } set { this[0, 0] = value; } }
        //
        // Summary:
        //     Gets or sets this[0,1].
        public double M01 { get { return this[0, 1]; } set { this[0, 1] = value; } }
        //
        // Summary:
        //     Gets or sets this[0,2].
        public double M02 { get { return this[0, 2]; } set { this[0, 2] = value; } }
        //
        // Summary:
        //     Gets or sets this[0,3].
        public double M03 { get { return this[0, 3]; } set { this[0, 3] = value; } }
        //
        // Summary:
        //     Gets or sets this[1,0].
        public double M10 { get { return this[1, 0]; } set { this[1, 0] = value; } }
        //
        // Summary:
        //     Gets or sets this[1,1].
        public double M11 { get { return this[1, 1]; } set { this[1, 1] = value; } }
        //
        // Summary:
        //     Gets or sets this[1,2].
        public double M12 { get { return this[1, 2]; } set { this[1, 2] = value; } }
        //
        // Summary:
        //     Gets or sets this[1,3].
        public double M13 { get { return this[1, 3]; } set { this[1, 3] = value; } }
        //
        // Summary:
        //     Gets or sets this[2,0].
        public double M20 { get { return this[2, 0]; } set { this[2, 0] = value; } }
        //
        // Summary:
        //     Gets or sets this[2,1].
        public double M21 { get { return this[2, 1]; } set { this[2, 1] = value; } }
        //
        // Summary:
        //     Gets or sets this[2,2].
        public double M22 { get { return this[2, 2]; } set { this[2, 2] = value; } }
        //
        // Summary:
        //     Gets or sets this[2,3].
        public double M23 { get { return this[2, 3]; } set { this[2, 3] = value; } }
        //
        // Summary:
        //     Gets or sets this[3,0].
        public double M30 { get { return this[3, 0]; } set { this[3, 0] = value; } }
        //
        // Summary:
        //     Gets or sets this[3,1].
        public double M31 { get { return this[3, 1]; } set { this[3, 1] = value; } }
        //
        // Summary:
        //     Gets or sets this[3,2].
        public double M32 { get { return this[3, 2]; } set { this[3, 2] = value; } }
        //
        // Summary:
        //     Gets or sets this[3,3].
        public double M33 { get { return this[3, 3]; } set { this[3, 3] = value; } }
        //
        // Summary:
        //     Return true if this Transform3D is the identity transform
        public bool IsIdentity
        {
            get
            {
                for (int row = 0; row < ROWS; row++)
                {
                    for (int col = 0; col < COLS; col++)
                    {
                        if (this[row, col] != (row == col ? 1.0 : 0.0))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        //
        // Summary:
        //     Gets a value indicating whether or not this Transform3D is a valid matrix. A valid
        //     transform matrix is not allowed to have any invalid numbers.
        public bool IsValid
        {
            get
            {
                for (int row = 0; row < ROWS; row++)
                {
                    for (int col = 0; col < COLS; col++)
                    {
                        if (double.IsNaN(this[row, col]))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        //
        // Summary:
        //     Gets a value indicating whether or not the Transform3D maintains similarity. The
        //     easiest way to think of Similarity is that any circle, when transformed, remains
        //     a circle. Whereas a non-similarity Transform3D deforms circles into ellipses.
        //!!!! public TransformSimilarityType SimilarityType { get; }
        //
        // Summary:
        //     The determinant of this 4x4 matrix.
        public double Determinant
        {
            get
            {
                return
                    M03 * M12 * M21 * M30 - M02 * M13 * M21 * M30 -
                    M03 * M11 * M22 * M30 + M01 * M13 * M22 * M30 +
                    M02 * M11 * M23 * M30 - M01 * M12 * M23 * M30 -
                    M03 * M12 * M20 * M31 + M02 * M13 * M20 * M31 +
                    M03 * M10 * M22 * M31 - M00 * M13 * M22 * M31 -
                    M02 * M10 * M23 * M31 + M00 * M12 * M23 * M31 +
                    M03 * M11 * M20 * M32 - M01 * M13 * M20 * M32 -
                    M03 * M10 * M21 * M32 + M00 * M13 * M21 * M32 +
                    M01 * M10 * M23 * M32 - M00 * M11 * M23 * M32 -
                    M02 * M11 * M20 * M33 + M01 * M12 * M20 * M33 +
                    M02 * M10 * M21 * M33 - M00 * M12 * M21 * M33 -
                    M01 * M10 * M22 * M33 + M00 * M11 * M22 * M33;
            }
        }
        //
        // Summary:
        //     Computes a change of basis transformation. A basis change is essentially a remapping
        //     of geometry from one coordinate system to another.
        //
        // Parameters:
        //   initialBasisX:
        //     can be any 3d basis.
        //
        //   initialBasisY:
        //     can be any 3d basis.
        //
        //   initialBasisZ:
        //     can be any 3d basis.
        //
        //   finalBasisX:
        //     can be any 3d basis.
        //
        //   finalBasisY:
        //     can be any 3d basis.
        //
        //   finalBasisZ:
        //     can be any 3d basis.
        //
        // Returns:
        //     A transformation matrix which orients geometry from one coordinate system to
        //     another on success. Transform3D.Unset on failure.
        public static Transform3D ChangeBasis(Vector3D initialBasisX, Vector3D initialBasisY, Vector3D initialBasisZ, Vector3D finalBasisX, Vector3D finalBasisY, Vector3D finalBasisZ)
        {
            return Identity;
        }
        //
        // Summary:
        //     Computes a change of basis transformation. A basis change is essentially a remapping
        //     of geometry from one coordinate system to another.
        //
        // Parameters:
        //   plane0:
        //     Coordinate system in which the geometry is currently described.
        //
        //   plane1:
        //     Target coordinate system in which we want the geometry to be described.
        //
        // Returns:
        //     A transformation matrix which orients geometry from one coordinate system to
        //     another on success. Transform3D.Unset on failure.
        public static Transform3D ChangeBasis(Plane3D plane0, Plane3D plane1)
        {
            return Identity;
        }
        //
        // Summary:
        //     Constructs a new Mirror transformation.
        //
        // Parameters:
        //   mirrorPlane:
        //     Plane that defines the mirror orientation and position.
        //
        // Returns:
        //     A transformation matrix which mirrors geometry in a specified plane.
        public static Transform3D Mirror(Plane3D mirrorPlane)
        {
            return Identity;
        }
        //
        // Summary:
        //     Create mirror transformation matrix The mirror transform maps a point Q to Q
        //     - (2*(Q-P)oN)*N, where P = pointOnMirrorPlane and N = normalToMirrorPlane.
        //
        // Parameters:
        //   pointOnMirrorPlane:
        //     Point on the mirror plane.
        //
        //   normalToMirrorPlane:
        //     Normal vector to the mirror plane.
        //
        // Returns:
        //     A transformation matrix which mirrors geometry in a specified plane.
        public static Transform3D Mirror(Point3D pointOnMirrorPlane, Vector3D normalToMirrorPlane)
        {
            return Identity;
        }
        //
        // Summary:
        //     Multiplies (combines) two transformations.
        //     This is the same as the * operator between two transformations.
        //
        // Parameters:
        //   a:
        //     First transformation.
        //
        //   b:
        //     Second transformation.
        //
        // Returns:
        //     A transformation matrix that combines the effect of both input transformations.
        //     The resulting Transform3D gives the same result as though you'd first apply B then
        //     A.
        public static Transform3D Multiply(Transform3D a, Transform3D b)
        {
            return new Transform3D
            {
                Matrix = new double[ROWS, COLS]
                {
                    {
                        a.M00 * b.M00 + a.M01 * b.M10 + a.M02 * b.M20 + a.M03 * b.M30,
                        a.M00 * b.M01 + a.M01 * b.M11 + a.M02 * b.M21 + a.M03 * b.M31,
                        a.M00 * b.M02 + a.M01 * b.M12 + a.M02 * b.M22 + a.M03 * b.M32,
                        a.M00 * b.M03 + a.M01 * b.M13 + a.M02 * b.M23 + a.M03 * b.M33
                    },
                    {
                        a.M10 * b.M00 + a.M11 * b.M10 + a.M12 * b.M20 + a.M13 * b.M30,
                        a.M10 * b.M01 + a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31,
                        a.M10 * b.M02 + a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32,
                        a.M10 * b.M03 + a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33
                    },
                    {
                        a.M20 * b.M00 + a.M21 * b.M10 + a.M22 * b.M20 + a.M23 * b.M30,
                        a.M20 * b.M01 + a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31,
                        a.M20 * b.M02 + a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32,
                        a.M20 * b.M03 + a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33
                    },
                    {
                        a.M30 * b.M00 + a.M31 * b.M10 + a.M32 * b.M20 + a.M33 * b.M30,
                        a.M30 * b.M01 + a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31,
                        a.M30 * b.M02 + a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32,
                        a.M30 * b.M03 + a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33
                    }
                }
            };
        }
        //
        // Summary:
        //     Constructs a projection transformation.
        //
        // Parameters:
        //   plane:
        //     Plane onto which everything will be perpendicularly projected.
        //
        // Returns:
        //     A transformation matrix which projects geometry onto a specified plane.
        public static Transform3D PlanarProjection(Plane3D plane)
        {
            return Identity;
        }
        //
        public static Transform3D PlaneToPlane(Plane3D plane0, Plane3D plane1)
        {
            return Identity;
        }
        //
        // Summary:
        //     Construct a projection onto a plane along a specific direction.
        //
        // Parameters:
        //   plane:
        //     Plane to project onto.
        //
        //   direction:
        //     Projection direction, must not be parallel to the plane.
        //
        // Returns:
        //     Projection transformation or identity transformation if projection could not
        //     be calculated.
        public static Transform3D ProjectAlong(Plane3D plane, Vector3D direction)
        {
            return Identity;
        }
        //
        // Summary:
        //     Constructs a new rotation transformation with specified angle and rotation center.
        //
        // Parameters:
        //   angleRadians:
        //     Angle (in Radians) of the rotation.
        //
        //   rotationCenter:
        //     Center point of rotation. Rotation axis is vertical.
        //
        // Returns:
        //     A transformation matrix which rotates geometry around an anchor point.
        public static Transform3D Rotation(double angleRadians, Point3D rotationCenter)
        {
            double sinAngle = Math.Sin(angleRadians);
            double cosAngle = Math.Cos(angleRadians);
            return new Transform3D
            {
                Matrix = new double[ROWS, COLS]
                {
                    { cosAngle, -sinAngle, 0.0, 0.0 },
                    { sinAngle, cosAngle, 0.0, 0.0 },
                    { 0.0, 0.0, 1.0, 0.0 },
                    { 0.0, 0.0, 0.0, 1.0 }
                }
            };
        }
        //
        // Summary:
        //     Constructs a new rotation transformation with specified angle, rotation center
        //     and rotation axis.
        //
        // Parameters:
        //   angleRadians:
        //     Angle (in Radians) of the rotation.
        //
        //   rotationAxis:
        //     Axis direction of rotation operation.
        //
        //   rotationCenter:
        //     Center point of rotation. Rotation axis is vertical.
        //
        // Returns:
        //     A transformation matrix which rotates geometry around an anchor point.
        public static Transform3D Rotation(double angleRadians, Vector3D rotationAxis, Point3D rotationCenter)
        {
            return Rotation(Math.Sin(angleRadians), Math.Cos(angleRadians), rotationAxis, rotationCenter);
        }
        //
        // Summary:
        //     Constructs a new rotation transformation with start and end directions and rotation
        //     center.
        //
        // Parameters:
        //   startDirection:
        //     A start direction.
        //
        //   endDirection:
        //     An end direction.
        //
        //   rotationCenter:
        //     A rotation center.
        //
        // Returns:
        //     A transformation matrix which rotates geometry around an anchor point.
        public static Transform3D Rotation(Vector3D startDirection, Vector3D endDirection, Point3D rotationCenter)
        {
            return Identity;
        }
        //
        // Summary:
        //     Constructs a transformation that maps X0 to X1, Y0 to Y1, Z0 to Z1.
        //
        // Parameters:
        //   x0:
        //     First "from" vector.
        //
        //   y0:
        //     Second "from" vector.
        //
        //   z0:
        //     Third "from" vector.
        //
        //   x1:
        //     First "to" vector.
        //
        //   y1:
        //     Second "to" vector.
        //
        //   z1:
        //     Third "to" vector.
        //
        // Returns:
        //     A rotation transformation value.
        public static Transform3D Rotation(Vector3D x0, Vector3D y0, Vector3D z0, Vector3D x1, Vector3D y1, Vector3D z1)
        {
            double Lx0 = x0.Length, Ly0 = y0.Length, Lz0 = z0.Length;
            double Lx1 = x1.Length, Ly1 = y1.Length, Lz1 = z0.Length;
            Vector3D Nx0 = x0 * (1 / Lx0), Ny0 = y0 * (1 / Ly0), Nz0 = z0 * (1 / Lz0);
            Vector3D Nx1 = x1 * (1 / Lx1), Ny1 = y1 * (1 / Ly1), Nz1 = z1 * (1 / Lz1);

            return Identity;
        }
        //
        // Summary:
        //     Constructs a new rotation transformation with specified angle, rotation center
        //     and rotation axis.
        //
        // Parameters:
        //   sinAngle:
        //     Sin of the rotation angle.
        //
        //   cosAngle:
        //     Cos of the rotation angle.
        //
        //   rotationAxis:
        //     Axis direction of rotation.
        //
        //   rotationCenter:
        //     Center point of rotation.
        //
        // Returns:
        //     A transformation matrix which rotates geometry around an anchor point.
        public static Transform3D Rotation(double sinAngle, double cosAngle, Vector3D rotationAxis, Point3D rotationCenter)
        {
            // Use the Rodrigues Rotation Formula
            Vector3D N = rotationAxis;
            N.Unitize();
            double Nx = N.X, Ny = N.Y, Nz = N.Z;
            double NxS = Nx * sinAngle, NyS = Ny * sinAngle, NzS = Nz * sinAngle;
            double NxC = Nx * cosAngle, NyC = Ny * cosAngle, NzC = Nz * cosAngle;
            double C1 = 1 - cosAngle;
            double NxC1 = Nx * C1, NyC1 = Ny * C1, NzC1 = Nz * C1;
            double Px = rotationCenter.X, Py = rotationCenter.Y, Pz = rotationCenter.Z;

            Transform3D R = new Transform3D
            {
                Matrix = new double[ROWS, COLS]
                {
                    { cosAngle + Nx * NxC1, Nx * NyC1 - NzS, NyS + Nx * NzC1, 0 },
                    { NzS + Nx * NyC1, cosAngle + Ny * NyC1, -NxS + Ny * NzC1, 0 },
                    { -NyS + Nx * NzC1, NxS + Ny * NzC1, cosAngle + Nz * NzC1, 0 },
                    { 0.0, 0.0, 0.0, 1.0 }
                }
            };

            // Translate origin to rotation center, perform rotation, restore origin
            R.M03 = Px - (R.M00 * Px + R.M01 * Py + R.M02 * Pz);
            R.M13 = Px - (R.M10 * Px + R.M11 * Py + R.M12 * Pz);
            R.M23 = Px - (R.M20 * Px + R.M21 * Py + R.M22 * Pz);

            return R;
        }
        //
        // Summary:
        //     Constructs a new non-uniform scaling transformation with a specified scaling
        //     anchor point.
        //
        // Parameters:
        //   plane:
        //     Defines the center and orientation of the scaling operation.
        //
        //   xScaleFactor:
        //     Scaling factor along the anchor plane X-Axis direction.
        //
        //   yScaleFactor:
        //     Scaling factor along the anchor plane Y-Axis direction.
        //
        //   zScaleFactor:
        //     Scaling factor along the anchor plane Z-Axis direction.
        //
        // Returns:
        //     A transformation matrix which scales geometry non-uniformly.
        public static Transform3D Scale(Plane3D plane, double xScaleFactor, double yScaleFactor, double zScaleFactor)
        {
            Transform3D Xform = Identity;
            Xform.M00 = xScaleFactor;
            Xform.M11 = yScaleFactor;
            Xform.M22 = zScaleFactor;
            return Xform;
        }
        //
        // Summary:
        //     Constructs a new uniform scaling transformation with a specified scaling anchor
        //     point.
        //
        // Parameters:
        //   anchor:
        //     Defines the anchor point of the scaling operation.
        //
        //   scaleFactor:
        //     Scaling factor in all directions.
        //
        // Returns:
        //     A transform matrix which scales geometry uniformly around the anchor point.
        public static Transform3D Scale(Point3D anchor, double scaleFactor)
        {
            double sf = 1 - scaleFactor;
            return new Transform3D
            {
                Matrix = new double[ROWS, COLS]
                {
                    { scaleFactor, 0.0, 0.0, anchor.X * sf },
                    { 0.0, scaleFactor, 0.0, anchor.Y * sf },
                    { 0.0, 0.0, scaleFactor, anchor.Z * sf},
                    { 0.0, 0.0, 0.0, 1.0 }
                }
            };
        }
        //
        // Summary:
        //     Constructs a Shear transformation.
        //
        // Parameters:
        //   plane:
        //     Base plane for shear.
        //
        //   x:
        //     Shearing vector along plane x-axis.
        //
        //   y:
        //     Shearing vector along plane y-axis.
        //
        //   z:
        //     Shearing vector along plane z-axis.
        //
        // Returns:
        //     A transformation matrix which shear geometry.
        public static Transform3D Shear(Plane3D plane, Vector3D x, Vector3D y, Vector3D z)
        {
            return Identity;
        }
        //
        // Summary:
        //     Constructs a new translation (move) tranformation. Right column is (dx, dy, dz,
        //     1.0).
        //
        // Parameters:
        //   dx:
        //     Distance to translate (move) geometry along the world X axis.
        //
        //   dy:
        //     Distance to translate (move) geometry along the world Y axis.
        //
        //   dz:
        //     Distance to translate (move) geometry along the world Z axis.
        //
        // Returns:
        //     A transform matrix which moves geometry with the specified distances.
        public static Transform3D Translation(double dx, double dy, double dz)
        {
            return new Transform3D
            {
                Matrix = new double[ROWS, COLS]
                {
                    { 1.0, 0.0, 0.0, dx },
                    { 0.0, 1.0, 0.0, dy },
                    { 0.0, 0.0, 1.0, dz },
                    { 0.0, 0.0, 0.0, 1.0 }
                }
            };
        }
        //
        // Summary:
        //     Constructs a new translation (move) transformation.
        //
        // Parameters:
        //   motion:
        //     Translation (motion) vector.
        //
        // Returns:
        //     A transform matrix which moves geometry along the motion vector.
        public static Transform3D Translation(Vector3D motion)
        {
            return Translation(motion.X, motion.Y, motion.Z);
        }
        //
        // Summary:
        //     Returns a deep copy of the transform. For languages that treat structures as
        //     value types, this can be accomplished by a simple assignment.
        //
        // Returns:
        //     A deep copy of this data structure.
        public Transform3D Clone()
        {
            return new Transform3D(this);
        }
        //
        // Summary:
        //     Compares this transform with another transform.
        //     M33 has highest value, then M32, etc..
        //
        // Parameters:
        //   other:
        //     Another transform.
        //
        // Returns:
        //     -1 if this < other; 0 if both are equal; 1 otherwise.
        public int CompareTo(Transform3D other)
        {
            for (int row = ROWS - 1; row >= 0; row--)
            {
                for (int col = COLS - 1; col >= 0; col--)
                {
                    if (this[row, col] < other[row, col])
                    {
                        return -1;
                    }
                    else if (this[row, col] > other[row, col])
                    {
                        return 1;
                    }
                }
            }
            return 0;
        }
        //
        // Summary:
        //     Determines if another transform equals this transform value.
        //
        // Parameters:
        //   other:
        //     Another transform.
        //
        // Returns:
        //     true if other has the same value as this transform; otherwise, false.
        public bool Equals(Transform3D other)
        {
            for (int row = ROWS - 1; row >= 0; row--)
            {
                for (int col = COLS - 1; col >= 0; col--)
                {
                    if (this[row, col] != other[row, col]) { return false; }
                }
            }
            return true;
        }
        //
        // Summary:
        //     Determines if another object is a transform and its value equals this transform
        //     value.
        //
        // Parameters:
        //   obj:
        //     Another object.
        //
        // Returns:
        //     true if obj is a transform and has the same value as this transform; otherwise,
        //     false.
        public override bool Equals(object obj)
        {
            return (obj is Transform3D transform) ? Equals(transform) : false;
        }
        //
        // Summary:
        //     Gets a non-unique hashing code for this transform.
        //
        // Returns:
        //     A number that can be used to hash this transform in a dictionary.
        public override int GetHashCode()
        {
            return Determinant.GetHashCode();
        }
        //
        // Summary:
        //     Return the matrix as a linear array of 16 float values
        //
        // Parameters:
        //   rowDominant:
        public float[] ToFloatArray(bool rowDominant)
        {
            float[] array = new float[ROWS * COLS];

            int index = 0;
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    array[index++] = (float)this[row, col];
                }
            }

            return array;
        }
        //
        // Summary:
        //     Returns a string representation of this transform.
        //
        // Returns:
        //     A textual representation.
        public override string ToString()
        {
            return Determinant.ToString();
        }
        //
        // Summary:
        //     Computes a new boundingbox that is the smallest axis aligned boundingbox that
        //     contains the transformed result of its 8 original corner points.
        //
        // Returns:
        //     A new bounding box.
        //!!!! public BoundingBox TransformBoundingBox(BoundingBox bbox);
        //
        // Summary:
        //     Given a list, an array or any enumerable set of points, computes a new array
        //     of tranformed points.
        //
        // Parameters:
        //   points:
        //     A list, an array or any enumerable set of points to be left untouched and copied.
        //
        // Returns:
        //     A new array.
        public Point3D[] TransformList(IEnumerable<Point3D> points)
        {
            int count = 0;
            foreach (Point3D point in points) { count++; }

            Point3D[] newpoints = new Point3D[count];
            int index = 0;
            foreach (Point3D point in points)
            {
                newpoints[index++] = this * point;
            }
            return newpoints;
        }
        //
        // Summary:
        //     Flip row/column values
        public Transform3D Transpose()
        {
            return new Transform3D
            {
                Matrix = new double[ROWS, COLS]
                {
                    { M00, M10, M20, M30 },
                    { M01, M11, M21, M31 },
                    { M02, M12, M22, M32 },
                    { M03, M13, M23, M33 }
                }
            };
        }

        //
        // Summary:
        //     Attempts to get the inverse transform of this transform.
        //
        // Parameters:
        //   inverseTransform:
        //     The inverse transform. This out reference will be assigned during this call.
        //
        // Returns:
        //     true on success. If false is returned and this Transform3D is Invalid, inverseTransform
        //     will be set to this Transform3D. If false is returned and this Transform3D is Valid,
        //     inverseTransform will be set to a pseudo inverse.
        public bool TryGetInverse(out Transform3D inverseTransform)
        {
            if (!IsValid)
            {
                inverseTransform = this;
                return false;
            }

            inverseTransform = new Transform3D
            {
                Matrix = new double[ROWS, COLS]
                {
                    {
                        M11 * M22 * M33 - M11 * M23 * M32 - M21 * M12 * M33 +
                        M21 * M13  * M32 + M31 * M12  * M23 - M31 * M13 * M22,

                        -M01 * M22 * M33 + M01 * M23 * M32 + M21 * M02 * M33 -
                        M21 * M03 * M32 - M31 * M02 * M23 + M31 * M03 * M22,

                        M01 * M12 * M33 - M01 * M13 * M32 - M11 * M02 * M33 +
                        M11 * M03 * M32 + M31 * M02 * M13 - M31 * M03 * M12,

                        -M01 * M12 * M23 + M01 * M13 * M22 + M11 * M02 * M23 -
                        M11 * M03 * M22 - M21 * M02 * M13 + M21 * M03 * M12
                    },
                    {
                        -M10 * M22 * M33 + M10 * M23 * M32 + M20 * M12 * M33 -
                        M20 * M13 * M32 - M30 * M12 * M23 + M30 * M13 * M22,

                        M00 * M22 * M33 - M00 * M23 * M32 - M20 * M02 * M33 +
                        M20 * M03 * M32 + M30 * M02 * M23 - M30 * M03 * M22,

                        -M00 * M12 * M33 + M00 * M13 * M32 + M10 * M02 * M33 -
                        M10 * M03 * M32 - M30 * M02 * M13 + M30 * M03 * M12,

                        M00 * M12 * M23 - M00 * M13 * M22 - M10 * M02 * M23 +
                        M10 * M03 * M22 + M20 * M02 * M13 - M20 * M03 * M12
                    },
                    {
                        M10 * M21 * M33 - M10 * M23 * M31 - M20 * M11 * M33 +
                        M20 * M13 * M31 + M30 * M11 * M23 - M30 * M13 * M21,

                        -M00 * M21 * M33 + M00 * M23 * M31 + M20 * M01 * M33 -
                        M20 * M03 * M31 - M30 * M01 * M23 + M30 * M03 * M21,

                        M00 * M11 * M33 - M00 * M13 * M31 - M10 * M01 * M33 +
                        M10 * M03 * M31 + M30 * M01 * M13 - M30 * M03 * M11,

                        -M00 * M11 * M23 + M00 * M13 * M21 + M10 * M01 * M23 -
                        M10 * M03 * M21 - M20 * M01 * M13 + M20 * M03 * M11
                    },
                    {
                        -M10 * M21 * M32 + M10 * M22 * M31 + M20 * M11 * M32 -
                        M20 * M12 * M31 - M30 * M11 * M22 + M30 * M12 * M21,

                        M00 * M21 * M32 - M00 * M22 * M31 - M20 * M01 * M32 +
                        M20 * M02 * M31 + M30 * M01 * M22 - M30 * M02 * M21,

                        -M00 * M11 * M32 + M00 * M12 * M31 + M10 * M01 * M32 -
                        M10 * M02 * M31 - M30 * M01 * M12 + M30 * M02 * M11,

                        M00 * M11 * M22 - M00 * M12 * M21 - M10 * M01 * M22 +
                        M10 * M02 * M21 + M20 * M01 * M12 - M20 * M02 * M11
                    }
                }
            };

            double determinate = M00 * inverseTransform.M00 + M01 * inverseTransform.M10 +
                M02 * inverseTransform.M20 + M03 * inverseTransform.M30;

            if (0 == determinate)
            {
                return false;
            }

            determinate = 1.0 / determinate;
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    inverseTransform[row, col] *= determinate;
                }
            }

            return true;
        }
        //
        // Summary:
        //     Multiplies (combines) two transformations.
        //
        // Parameters:
        //   a:
        //     First transformation.
        //
        //   b:
        //     Second transformation.
        //
        // Returns:
        //     A transformation matrix that combines the effect of both input transformations.
        //     The resulting Transform3D gives the same result as though you'd first apply A then
        //     B.
        public static Transform3D operator *(Transform3D a, Transform3D b)
        {
            return Multiply(a, b);
        }
        //
        // Summary:
        //     Multiplies a transformation by a point and gets a new point.
        //
        // Parameters:
        //   m:
        //     A transformation.
        //
        //   p:
        //     A point.
        //
        // Returns:
        //     The tranformed point.
        public static Point3D operator *(Transform3D m, Point3D p)
        {
            return new Point3D(
                p.X * m.M00 + p.Y * m.M01 + p.Z * m.M02 + m.M03,
                p.X * m.M10 + p.Y * m.M11 + p.Z * m.M12 + m.M13,
                p.X * m.M20 + p.Y * m.M21 + p.Z * m.M22 + m.M23);
        }
        //
        // Summary:
        //     Multiplies a transformation by a vector and gets a new vector.
        //
        // Parameters:
        //   m:
        //     A transformation.
        //
        //   v:
        //     A vector.
        //
        // Returns:
        //     The tranformed vector.
        public static Vector3D operator *(Transform3D m, Vector3D v)
        {
            return new Vector3D(
                v.X * m.M00 + v.Y * m.M01 + v.Z * m.M02 + m.M03,
                v.X * m.M10 + v.Y * m.M11 + v.Z * m.M12 + m.M13,
                v.X * m.M20 + v.Y * m.M21 + v.Z * m.M22 + m.M23);
        }
        //
        // Summary:
        //     Determines if two transformations are equal in value.
        //
        // Parameters:
        //   a:
        //     A tranform.
        //
        //   b:
        //     Another transform.
        //
        // Returns:
        //     true if transforms are equal; otherwise false.
        public static bool operator ==(Transform3D a, Transform3D b)
        {
            return a.Equals(b);
        }
        //
        // Summary:
        //     Determines if two transformations are different in value.
        //
        // Parameters:
        //   a:
        //     A tranform.
        //
        //   b:
        //     Another tranform.
        //
        // Returns:
        //     true if transforms are different; otherwise false.
        public static bool operator !=(Transform3D a, Transform3D b)
        {
            return !a.Equals(b);
        }

        int IComparable.CompareTo(object obj)
        {
            return (obj is Transform3D transform) ? CompareTo(transform) : -1;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public static bool operator <(Transform3D left, Transform3D right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(Transform3D left, Transform3D right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(Transform3D left, Transform3D right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(Transform3D left, Transform3D right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}