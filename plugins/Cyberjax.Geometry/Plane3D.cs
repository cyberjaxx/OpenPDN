using System;
using System.Collections.Generic;

namespace Cyberjax.Geometry
{
    //
    // Summary:
    //     Represents the value of a center point and two axes in a plane in three dimensions.
    public struct Plane3D : IEquatable<Plane3D>, IEpsilonComparable<Plane3D>, ICloneable
    {
        //
        // Summary:
        //     Enumerates all possible outcomes of a Least-Squares plane fitting operation.
        public enum PlaneFitResult
        {
            //
            // Summary:
            //     No plane could be found.
            Failure = -1,
            //
            // Summary:
            //     A plane was successfully fitted.
            Success = 0,
            //
            // Summary:
            //     A valid plane was found, but it is an inconclusive result. This might happen
            //     with co-linear points for example.
            Inconclusive = 1
        }

        private Point3D m_origin;

        //
        // Summary:
        //     Copy constructor.
        //     This is nothing special and performs the same as assigning to another variable.
        //
        // Parameters:
        //   other:
        //     The source plane value.
        public Plane3D(Plane3D other)
        {
            m_origin = other.Origin;
            XAxis = other.XAxis;
            YAxis = other.YAxis;
            ZAxis = other.ZAxis;
        }
        //
        // Summary:
        //     Constructs a plane from a point and a normal vector.
        //
        // Parameters:
        //   origin:
        //     Origin point of the plane.
        //
        //   normal:
        //     Non-zero normal to the plane.
        public Plane3D(Point3D origin, Vector3D normal)
        {
            m_origin = origin;
            XAxis = Vector3D.CrossProduct(Vector3D.ZAxis, normal);
            YAxis = Vector3D.CrossProduct(normal, XAxis);
            ZAxis = normal;
        }
        //
        // Summary:
        //     Constructs a plane from a point and two vectors in the plane.
        //
        // Parameters:
        //   origin:
        //     Origin point of the plane.
        //
        //   xDirection:
        //     Non-zero vector in the plane that determines the x-axis direction.
        //
        //   yDirection:
        //     Non-zero vector not parallel to x_dir that is used to determine the yaxis direction.
        //     y_dir does not need to be perpendicular to x_dir.
        public Plane3D(Point3D origin, Vector3D xDirection, Vector3D yDirection)
        {
            m_origin = origin;
            XAxis = xDirection;
            YAxis = yDirection;
            ZAxis = Vector3D.CrossProduct(XAxis, YAxis);
        }
        //
        // Summary:
        //     Initializes a plane from three non-colinear points.
        //
        // Parameters:
        //   origin:
        //     Origin point of the plane.
        //
        //   xPoint:
        //     Second point in the plane. The x-axis will be parallel to x_point-origin.
        //
        //   yPoint:
        //     Third point on the plane that is not colinear with the first two points. yaxis*(y_point-origin)
        //     will be > 0.
        public Plane3D(Point3D origin, Point3D xPoint, Point3D yPoint)
        {
            m_origin = origin;
            XAxis = xPoint - m_origin;
            ZAxis = Vector3D.CrossProduct(XAxis, yPoint - m_origin);
            YAxis = Vector3D.CrossProduct(ZAxis, XAxis);
        }
        //
        // Summary:
        //     Constructs a plane from an equation ax+by+cz=d.
        public Plane3D(double a, double b, double c, double d)
        {
            ZAxis = new Vector3D(a, b, c);
            Vector3D normal = new Vector3D(ZAxis);
            normal.Unitize();
            m_origin = new Point3D(normal * d);
            if ((c != 0) && (-a != b))
            {
                XAxis = new Vector3D(-c, -c, a + b);
            }
            else
            {
                XAxis = new Vector3D(b + c, -a, -a);
            }
            YAxis = Vector3D.CrossProduct(ZAxis, XAxis);
        }

        //
        // Summary:
        //     plane coincident with the World YZ plane.
        public static Plane3D WorldYZ
        {
            get { return new Plane3D(Point3D.Origin, Vector3D.XAxis); }
        }
        //
        // Summary:
        //     plane coincident with the World XY plane.
        public static Plane3D WorldXY
        {
            get { return new Plane3D(Point3D.Origin, Vector3D.ZAxis); }
        }
        //
        // Summary:
        //     Gets a plane that contains Unset origin and axis vectors.
        public static Plane3D Unset
        {
            get
            {
                return new Plane3D
                {
                    Origin = Point3D.Unset,
                    XAxis = Vector3D.Unset,
                    YAxis = Vector3D.Unset,
                    ZAxis = Vector3D.Unset
                };
            }
        }
        //
        // Summary:
        //     plane coincident with the World ZX plane.
        public static Plane3D WorldZX
        {
            get { return new Plane3D(Point3D.Origin, Vector3D.YAxis); }
        }
        //
        // Summary:
        //     Gets or sets the origin point of this plane.
        public Point3D Origin
        {
            get { return m_origin; }
            set { m_origin = value; }
        }
        //
        // Summary:
        //     Gets or sets the Z axis vector of this plane.
        public Vector3D ZAxis { get; set; }
        // Summary:
        //     Gets or sets the Y axis vector of this plane.
        public Vector3D YAxis { get; set; }
        //
        // Summary:
        //     Gets or sets the X axis vector of this plane.
        public Vector3D XAxis { get; set; }
        //
        // Summary:
        //     Gets or sets the Z coordinate of the origin of this plane.
        public double OriginZ
        {
            get { return m_origin.Z; }
            set { m_origin.Z = value; }
        }
        //
        // Summary:
        //     Gets or sets the Y coordinate of the origin of this plane.
        public double OriginY
        {
            get { return m_origin.Y; }
            set { m_origin.Y = value; }
        }
        //
        // Summary:
        //     Gets or sets the X coordinate of the origin of this plane.
        public double OriginX
        {
            get { return m_origin.Y; }
            set { m_origin.Y = value; }
        }
        //
        // Summary:
        //     Gets the normal of this plane. This is essentially the ZAxis of the plane.
        public Vector3D Normal
        {
            get { return ZAxis; }
        }
        //
        // Summary:
        //     Gets a value indicating whether or not this is a valid plane. A plane is considered
        //     to be valid when all fields contain reasonable information and the equation jibes
        //     with point and zaxis.
        public bool IsValid
        {
            get
            {
                bool allValid = Origin.IsValid && XAxis.IsValid && YAxis.IsValid && ZAxis.IsValid;
                bool anyZero = XAxis.IsZero && YAxis.IsZero && ZAxis.IsZero;
                return allValid && !anyZero && !IsAnyAxisParallel();
            }
        }

        // this checks for exactly parallel axis
        private bool IsAnyAxisParallel()
        {
            double xyx = (XAxis.X * YAxis.X);
            double xyy = (XAxis.Y * YAxis.Y);
            double xyz = (XAxis.Z * YAxis.Z);
            if (xyx == xyy && xyx == xyz) { return false; }

            double xzx = (XAxis.X * ZAxis.X);
            double xzy = (XAxis.Y * ZAxis.Y);
            double xzz = (XAxis.Z * ZAxis.Z);
            if (xzx == xzy && xzx == xzz) { return false; }

            double yzx = (YAxis.X * ZAxis.X);
            double yzy = (YAxis.Y * ZAxis.Y);
            double yzz = (YAxis.Z * ZAxis.Z);
            if (yzx == yzy && yzx == yzz) { return false; }

            return false;
        }

        //
        // Summary:
        //     Fit a plane through a collection of points.
        //
        // Parameters:
        //   points:
        //     Points to fit to.
        //
        //   plane:
        //     Resulting plane.
        //
        // Returns:
        //     A value indicating the result of the operation.
        //**** public static PlaneFitResult FitplaneToPoints(IEnumerable<Point3D> points, out plane plane);
        //
        // Summary:
        //     Fit a plane through a collection of points.
        //
        // Parameters:
        //   points:
        //     Points to fit to.
        //
        //   plane:
        //     Resulting plane.
        //
        //   maximumDeviation:
        //     The distance from the furthest point to the plane.
        //
        // Returns:
        //     A value indicating the result of the operation.
        //**** public static PlaneFitResult FitplaneToPoints(IEnumerable<Point3D> points, out plane plane, out double maximumDeviation);
        //
        // Summary:
        //     Returns a deep copy of this instance.
        //
        // Returns:
        //     A plane with the same values as this item.
        public Plane3D Clone()
        {
            return new Plane3D(this);
        }
        //
        // Summary:
        //     Gets the parameters of the point on the plane closest to a test point.
        //
        // Parameters:
        //   testPoint:
        //     Point to get close to.
        //
        //   s:
        //     Parameter along plane X-direction.
        //
        //   t:
        //     Parameter along plane Y-direction.
        //
        // Returns:
        //     true if a parameter could be found, false if the point could not be projected
        //     successfully.
        //**** public bool ClosestParameter(Point3D testPoint, out double s, out double t);
        //
        // Summary:
        //     Gets the point on the plane closest to a test point.
        //
        // Parameters:
        //   testPoint:
        //     Point to get close to.
        //
        // Returns:
        //     The point on the plane that is closest to testPoint, or Point3D.Unset on failure.
        public Point3D ClosestPoint(Point3D testPoint)
        {
            Vector3D vector = testPoint - Origin;
            Vector3D normal = Normal;
            normal.Unitize();
            double distance = normal * vector;
            return testPoint - (normal * distance);
        }
        //
        // Summary:
        //     Returns the signed minimum and maximum distances from bbox to this plane.
        //
        // Parameters:
        //   bbox:
        //     bounding box to get distances from
        //
        //   min:
        //     minimum signed distance from plane to box
        //
        //   max:
        //     maximum signed distance from plane to box
        //
        // Returns:
        //     false if plane has zero length normal
        //**** public bool DistanceTo(BoundingBox bbox, out double min, out double max)
        //
        // Summary:
        //     Returns the signed distance from testPoint to its projection onto this plane.
        //     If the point is below the plane, a negative distance is returned.
        //
        // Parameters:
        //   testPoint:
        //     Point to test.
        //
        // Returns:
        //     Signed distance from this plane to testPoint.
        public double DistanceTo(Point3D testPoint)
        {
            Vector3D vector = testPoint - Origin;
            Vector3D normal = Normal;
            normal.Unitize();
            return normal * vector;
        }
        //
        // Summary:
        //     Check that all values in other are within epsilon of the values in this
        //
        // Parameters:
        //   other:
        //
        //   epsilon:
        public bool EpsilonEquals(Plane3D other, double epsilon)
        {
            return Origin.EpsilonEquals(other.Origin, epsilon) &&
                XAxis.EpsilonEquals(other.XAxis, epsilon) &&
                YAxis.EpsilonEquals(other.YAxis, epsilon) &&
                ZAxis.EpsilonEquals(other.ZAxis, epsilon);
        }
        //
        // Summary:
        //     Determines if an object is a plane and has the same components as this plane.
        //
        // Parameters:
        //   obj:
        //     An object.
        //
        // Returns:
        //     true if obj is a plane and has the same components as this plane; false otherwise.
        public override bool Equals(object obj)
        {
            return (obj is Plane3D plane) ? Equals(plane) : false;
        }
        //
        // Summary:
        //     Determines if another plane has the same components as this plane.
        //
        // Parameters:
        //   plane:
        //     A plane.
        //
        // Returns:
        //     true if plane has the same components as this plane; false otherwise.
        public bool Equals(Plane3D plane)
        {
            return Origin.Equals(plane.Origin) && XAxis.Equals(plane.XAxis) && YAxis.Equals(plane.YAxis) && ZAxis.Equals(plane.ZAxis);
        }
        //
        // Summary:
        //     Extends this plane through a bounding box.
        //
        // Parameters:
        //   box:
        //     A box to use as minimal extension boundary.
        //
        //   s:
        //     If this function returns true, the s parameter returns the Interval on the plane
        //     along the X direction that will encompass the Box.
        //
        //   t:
        //     If this function returns true, the t parameter returns the Interval on the plane
        //     along the Y direction that will encompass the Box.
        //
        // Returns:
        //     true on success, false on failure.
        //**** public bool ExtendThroughBox(BoundingBox box, out Interval s, out Interval t);
        //
        // Summary:
        //     Extend this plane through a Box.
        //
        // Parameters:
        //   box:
        //     A box to use for extension.
        //
        //   s:
        //     If this function returns true, the s parameter returns the Interval on the plane
        //     along the X direction that will encompass the Box.
        //
        //   t:
        //     If this function returns true, the t parameter returns the Interval on the plane
        //     along the Y direction that will encompass the Box.
        //
        // Returns:
        //     true on success, false on failure.
        //**** public bool ExtendThroughBox(Box box, out Interval s, out Interval t);
        //
        // Summary:
        //     Flip this plane by swapping out the X and Y axes and inverting the Z axis.
        public void Flip()
        {
            Vector3D temp = XAxis;
            XAxis = YAxis;
            YAxis = temp;
            temp = ZAxis; temp.Z = -temp.Z;
            ZAxis = temp;
        }
        //
        // Summary:
        //     Gets a non-unique hashing code for this entity.
        //
        // Returns:
        //     A particular number for a specific instance of plane.
        public override int GetHashCode()
        {
            return Origin.GetHashCode() ^ XAxis.GetHashCode() ^ YAxis.GetHashCode() ^ ZAxis.GetHashCode();
        }
        //
        // Summary:
        //     Gets the plane equation for this plane in the format of Ax+By+Cz+D=0.
        //
        // Returns:
        //     Array of four values.
        public double[] GetPlaneEquation()
        {
            double D;
            if (0 != ZAxis.Z) { D = Origin.Z / ZAxis.Z; }
            else if (0 != ZAxis.Y) { D = Origin.Y / ZAxis.Y; }
            else { D = Origin.X / ZAxis.X; }
            return new double[4] { ZAxis.X, ZAxis.Y, ZAxis.Z, D };
        }
        //
        // Summary:
        //     Evaluate a point on the plane.
        //
        // Parameters:
        //   u:
        //     evaulation parameter.
        //
        //   v:
        //     evaulation parameter.
        //
        // Returns:
        //     plane.origin + u*plane.xaxis + v*plane.yaxis.
        public Point3D PointAt(double u, double v)
        {
            return Origin + u * XAxis + v * YAxis;
        }
        //
        // Summary:
        //     Evaluate a point on the plane.
        //
        // Parameters:
        //   u:
        //     evaulation parameter.
        //
        //   v:
        //     evaulation parameter.
        //
        //   w:
        //     evaulation parameter.
        //
        // Returns:
        //     plane.origin + u*plane.xaxis + v*plane.yaxis + z*plane.zaxis.
        public Point3D PointAt(double u, double v, double w)
        {
            return Origin + u * XAxis + v * YAxis + w * ZAxis;
        }
        //
        // Summary:
        //     Convert a point from World space coordinates into plane space coordinates.
        //
        // Parameters:
        //   ptSample:
        //     World point to remap.
        //
        //   ptplane:
        //     Point in plane (s,t,d) coordinates.
        //
        // Returns:
        //     true on success, false on failure.
        //
        // Remarks:
        //     D stands for distance, not disease.
        public bool RemapToPlaneSpace(Point3D ptSample, out Point3D ptPlane)
        {
            Point3D point = (Point3D)(ptSample - Origin);
            Vector3D XUnit = XAxis; XUnit.Unitize();
            Vector3D YUnit = YAxis; YUnit.Unitize();
            Vector3D ZUnit = ZAxis; ZUnit.Unitize();
            ptPlane = point;
            return true;
        }
        //
        // Summary:
        //     Rotate the plane about a custom anchor point.
        //
        // Parameters:
        //   sinAngle:
        //     Sin(angle)
        //
        //   cosAngle:
        //     Cos(angle)
        //
        //   axis:
        //     Axis of rotation.
        //
        //   centerOfRotation:
        //     Center of rotation.
        //
        // Returns:
        //     true on success, false on failure.
        public bool Rotate(double sinAngle, double cosAngle, Vector3D axis, Point3D centerOfRotation)
        {
            Transform(Transform3D.Rotation(sinAngle, cosAngle, axis, centerOfRotation));
            return true;
        }
        //
        // Summary:
        //     Rotate the plane about a custom anchor point.
        //
        // Parameters:
        //   angle:
        //     Angle in radians.
        //
        //   axis:
        //     Axis of rotation.
        //
        //   centerOfRotation:
        //     Center of rotation.
        //
        // Returns:
        //     true on success, false on failure.
        public bool Rotate(double angle, Vector3D axis, Point3D centerOfRotation)
        {
            return Rotate(Math.Sin(angle), Math.Cos(angle), axis, centerOfRotation);
        }
        //
        // Summary:
        //     Rotate the plane about its origin point.
        //
        // Parameters:
        //   angle:
        //     Angle in radians.
        //
        //   axis:
        //     Axis of rotation.
        //
        // Returns:
        //     true on success, false on failure.
        public bool Rotate(double angle, Vector3D axis)
        {
            return Rotate(angle, axis, Origin);
        }
        //
        // Summary:
        //     Rotate the plane about its origin point.
        //
        // Parameters:
        //   sinAngle:
        //     Sin(angle).
        //
        //   cosAngle:
        //     Cos(angle).
        //
        //   axis:
        //     Axis of rotation.
        //
        // Returns:
        //     true on success, false on failure.
        public bool Rotate(double sinAngle, double cosAngle, Vector3D axis)
        {
            return Rotate(sinAngle, cosAngle, axis, Origin);
        }
        //
        // Summary:
        //     Constructs the string representation of this plane.
        //
        // Returns:
        //     Text.
        public override string ToString()
        {
            return string.Format("{0} : {1} : {2} : {3}",
                Origin.ToString(), XAxis.ToString(), YAxis.ToString(), ZAxis.ToString());
        }
        //
        // Summary:
        //     Transform the plane with a Transformation matrix.
        //
        // Parameters:
        //   xform:
        //     Transformation to apply to plane.
        //
        // Returns:
        //     true on success, false on failure.
        public bool Transform(Transform3D xform)
        {
            Origin.Transform(xform);
            XAxis.Transform(xform);
            YAxis.Transform(xform);
            ZAxis.Transform(xform);
            return true;
        }
        //
        // Summary:
        //     Translate (move) the plane along a vector.
        //
        // Parameters:
        //   delta:
        //     Translation (motion) vector.
        //
        // Returns:
        //     true on success, false on failure.
        public bool Translate(Vector3D delta)
        {
            Origin += delta;
            return true;
        }
        //
        // Summary:
        //     Update Equations
        //
        // Returns:
        //     bool
        public bool UpdateEquation()
        {
            return true;
        }
        //
        // Summary:
        //     Get the value of the plane equation at the point.
        //
        // Parameters:
        //   p:
        //     evaulation point.
        //
        // Returns:
        //     returns pe[0]*p.X + pe[1]*p.Y + pe[2]*p.Z + pe[3] where pe[0], pe[1], pe[2] and
        //     pe[3] are the coeeficients of the plane equation.
        //**** public double ValueAt(Point3D p);
        //
        // Summary:
        //     Determines if two planes are equal.
        //
        // Parameters:
        //   a:
        //     A first plane.
        //
        //   b:
        //     A second plane.
        //
        // Returns:
        //     true if the two planes have all equal components; false otherwise.
        public static bool operator ==(Plane3D a, Plane3D b)
        {
            return a.Equals(b);
        }
        //
        // Summary:
        //     Determines if two planes are different.
        //
        // Parameters:
        //   a:
        //     A first plane.
        //
        //   b:
        //     A second plane.
        //
        // Returns:
        //     true if the two planes have any different componet components; false otherwise.
        public static bool operator !=(Plane3D a, Plane3D b)
        {
            return !a.Equals(b);
        }

        object ICloneable.Clone()
        {
            return new Plane3D(this);
        }

    }
}
