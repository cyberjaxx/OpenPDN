using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Cyberjax.Geometry
{
    public struct Box3D : ISerializable, IEquatable<Box3D>, IComparable<Box3D>, IComparable, IEpsilonComparable<Box3D>, ICloneable
    {
        private Point3D m_location;
        private Vector3D m_size;

        public Point3D Location
        {
            get { return m_location; }
            set { m_location = value; }
        }

        public Vector3D Size
        {
            get { return m_size; }
            set { m_size = value; }
        }

        public Point3D Point1
        {
            get { return new Point3D(m_location); }
            set { m_location = value; }
        }

        public Point3D Point2
        {
            get { return m_location + m_size; }
            set { m_size = value - m_location; }
        }

        public Box3D(Point3D location, Vector3D size)
        {
            m_location = new Point3D(location);
            m_size = new Vector3D(size);
        }

        public Box3D(Point3D point1, Point3D point2)
        {
            m_location = new Point3D(point1);
            m_size = point2 - point1;
        }

        public Box3D(Box3D box)
        {
            m_location = new Point3D(box.Location);
            m_size = new Vector3D(box.Size);
        }

        object ICloneable.Clone()
        {
            return new Box3D(this);
        }

        public static Box3D Empty
        {
            get
            {
                return new Box3D
                {
                    Location = Point3D.Origin,
                    Size = Vector3D.Zero
                };
            }
        }

        public static Box3D Default
        {
            get
            {
                return new Box3D
                {
                    Location = new Point3D(-1.0, -1.0, -1.0),
                    Size = new Vector3D(+2.0, +2.0, +2.0)
                };
            }
        }

        public static Box3D Unset
        {
            get
            {
                return new Box3D
                {
                    Location = Point3D.Unset,
                    Size = Vector3D.Unset
                };
            }
        }

        public Point3D Center
        {
            get
            {
                return new Point3D(Location.X + Size.X / 2,
                    Location.Y + Size.Y / 2, Location.Z + Size.Z / 2);
            }
        }

        public void Scale(double t)
        {
            Size *= t;
        }

        public void Scale(double scaleX, double scaleY, double scaleZ)
        {
            m_size.Scale(scaleX, scaleY, scaleZ);
        }

        public void Translate(double deltaX, double deltaY, double deltaZ)
        {
            m_location.Translate(deltaX, deltaY, deltaZ);
        }

        public static Box3D Multiply(Box3D box, double t)
        {
            return new Box3D(box.Location, box.Size * t);
        }

        public static Box3D Divide(Box3D box, double t)
        {
            return new Box3D(box.Location, box.Size / t);
        }

        public static Box3D Add(Box3D box, Vector3D vector)
        {
            return new Box3D(box.Location + vector, box.Size);
        }

        public static Box3D Subtract(Box3D box, Vector3D vector)
        {
            return new Box3D(box.Location - vector, box.Size);
        }

        public static Box3D operator *(Box3D box, double t)
        {
            return Multiply(box, t);
        }

        public static Box3D operator *(double t, Box3D box)
        {
            return Multiply(box, t);
        }

        public static Box3D operator /(Box3D box, double t)
        {
            return Divide(box, t);
        }

        public static Box3D operator +(Box3D box, Vector3D vector)
        {
            return Add(box, vector);
        }

        public static Box3D operator +(Vector3D vector, Box3D box)
        {
            return Add(box, vector);
        }

        public static Box3D operator -(Box3D box, Vector3D vector)
        {
            return Subtract(box, vector);
        }

        public bool Equals(Box3D box)
        {
            return (Location == box.Location && Size == box.Size);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (typeof(Box3D) != obj.GetType())
                return false;
            else
                return Equals((Box3D)obj);
        }

        public int CompareTo(Box3D other)
        {
            int result = Size.Length.CompareTo(other.Size.Length);
            if (0 != result) { return result; };
            return Location.CompareTo(other.Location);
        }

        public static bool operator ==(Box3D a, Box3D b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Box3D a, Box3D b)
        {
            return !a.Equals(b);
        }

        public static bool operator <(Box3D a, Box3D b)
        {
            return -1 == a.CompareTo(b);
        }

        public static bool operator >(Box3D a, Box3D b)
        {
            return 1 == a.CompareTo(b);
        }

        public static bool operator <=(Box3D a, Box3D b)
        {
            return 1 != a.CompareTo(b);
        }

        public static bool operator >=(Box3D a, Box3D b)
        {
            return -1 != a.CompareTo(b);
        }

        int IComparable.CompareTo(object obj)
        {
            {
                if (obj == null)
                    return -1;

                if (typeof(Box3D) != obj.GetType())
                    return -1;
                else
                    return CompareTo((Box3D)obj);
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        bool IEpsilonComparable<Box3D>.EpsilonEquals(Box3D other, double epsilon)
        {
            return Location.EpsilonEquals(other.Location, epsilon) &&
                Size.EpsilonEquals(other.Size, epsilon);
        }

        public override int GetHashCode()
        {
            return Location.GetHashCode() ^ Size.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} : {1}", Location.ToString(), Size.ToString());
        }
    }
}
