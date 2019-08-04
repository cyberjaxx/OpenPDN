using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

namespace Cyberjax.Geometry
{
    //
    // Summary:
    //     Represents an interval in one-dimensional space, that is defined as two extrema
    //     or bounds.
    [DebuggerDisplay("({m_t0}, {m_t1})")]

    public struct Interval : ISerializable, IEquatable<Interval>, IComparable<Interval>, IComparable, IEpsilonComparable<Interval>
    {
        //
        // Summary:
        //     Initializes a new instance copying the other instance values.
        //
        // Parameters:
        //   other:
        //     The Rhino.Geometry.Interval to use as a base.
        public Interval(Interval other);
        //
        // Summary:
        //     Initializes a new instance of the Rhino.Geometry.Interval class.
        //
        // Parameters:
        //   t0:
        //     The first value.
        //
        //   t1:
        //     The second value.
        public Interval(double t0, double t1);

        //
        // Summary:
        //     Gets or sets the indexed bound of this Interval.
        //
        // Parameters:
        //   index:
        //     Bound index (0 = lower; 1 = upper).
        public double this[int index] { get; set; }

        //
        // Summary:
        //     Gets an Interval whose limits are RhinoMath.UnsetValue.
        public static Interval Unset { get; }
        //
        // Summary:
        //     Returns true if T0 == T1 != ON.UnsetValue.
        public bool IsSingleton { get; }
        //
        // Summary:
        //     Gets a value indicating whether or not this Interval is valid. Valid intervals
        //     must contain valid numbers.
        public bool IsValid { get; }
        //
        // Summary:
        //     Gets the signed length of the numeric range. If the interval is decreasing, a
        //     negative length will be returned.
        public double Length { get; }
        //
        // Summary:
        //     Gets the average of T0 and T1.
        public double Mid { get; }
        //
        // Summary:
        //     Gets the larger of T0 and T1.
        public double Max { get; }
        //
        // Summary:
        //     Gets the smaller of T0 and T1.
        public double Min { get; }
        //
        // Summary:
        //     Gets or sets the upper bound of the Interval.
        public double T1 { get; set; }
        //
        // Summary:
        //     Gets or sets the lower bound of the Interval.
        public double T0 { get; set; }
        //
        // Summary:
        //     Returns true if T0 < T1.
        public bool IsIncreasing { get; }
        //
        // Summary:
        //     Returns true if T[0] > T[1].
        public bool IsDecreasing { get; }

        //
        // Summary:
        //     Returns a new Interval that is the Intersection of the two input Intervals.
        //
        // Parameters:
        //   a:
        //     The first input interval.
        //
        //   b:
        //     The second input interval.
        //
        // Returns:
        //     If the intersection is not empty, then intersection = [max(a.Min(),b.Min()),
        //     min(a.Max(),b.Max())] The interval [ON.UnsetValue,ON.UnsetValue] is considered
        //     to be the empty set interval. The result of any intersection involving an empty
        //     set interval or disjoint intervals is the empty set interval.
        public static Interval FromIntersection(Interval a, Interval b);
        //
        // Summary:
        //     Returns a new Interval which contains both inputs.
        //
        // Parameters:
        //   a:
        //     The first input interval.
        //
        //   b:
        //     The second input interval.
        //
        // Returns:
        //     The union of an empty set and an increasing interval is the increasing interval.
        //     The union of two empty sets is empty.
        //     The union of an empty set an a non-empty interval is the non-empty interval.
        //     The union of two non-empty intervals is [min(a.Min(),b.Min()), max(a.Max(),b.Max())]
        public static Interval FromUnion(Interval a, Interval b);
        //
        // Summary:
        //     Compares this Rhino.Geometry.Interval with another interval.
        //     The lower bound has first evaluation priority.
        //
        // Parameters:
        //   other:
        //     The other Rhino.Geometry.Interval to compare with.
        //
        // Returns:
        //     0: if this is identical to other
        //     -1: if this[0] < other[0]
        //     +1: if this[0] > other[0]
        //     -1: if this[0] == other[0] and this[1] < other[1]
        //     +1: if this[0] == other[0] and this[1] > other[1]
        //     .
        public int CompareTo(Interval other);
        //
        // Summary:
        //     Check that all values in other are within epsilon of the values in this
        //
        // Parameters:
        //   other:
        //
        //   epsilon:
        public bool EpsilonEquals(Interval other, double epsilon);
        //
        // Summary:
        //     Determines whether the specified Rhino.Geometry.Interval is equal to the current
        //     Rhino.Geometry.Interval, comparing by value.
        //
        // Parameters:
        //   other:
        //     The other interval to compare with.
        //
        // Returns:
        //     true if obj is an Rhino.Geometry.Interval and has the same bounds; false otherwise.
        public bool Equals(Interval other);
        //
        // Summary:
        //     Determines whether the specified System.Object is equal to the current Rhino.Geometry.Interval,
        //     comparing by value.
        //
        // Parameters:
        //   obj:
        //     The other object to compare with.
        //
        // Returns:
        //     true if obj is an Rhino.Geometry.Interval and has the same bounds; false otherwise.
        public override bool Equals(object obj);
        //
        // Summary:
        //     Computes the hash code for this Rhino.Geometry.Interval object.
        //
        // Returns:
        //     A hash value that might be equal for two different Rhino.Geometry.Interval values.
        public override int GetHashCode();
        //
        // Summary:
        //     Grows the Rhino.Geometry.Interval to include the given number.
        //
        // Parameters:
        //   value:
        //     Number to include in this interval.
        public void Grow(double value);
        //
        // Summary:
        //     Tests another interval for Interval inclusion.
        //
        // Parameters:
        //   interval:
        //     Interval to test.
        //
        // Returns:
        //     true if the other interval is contained within or is coincident with the limits
        //     of this Interval; otherwise false.
        public bool IncludesInterval(Interval interval);
        //
        // Summary:
        //     Tests another interval for Interval inclusion.
        //
        // Parameters:
        //   interval:
        //     Interval to test.
        //
        //   strict:
        //     If true, the other interval must be fully on the inside of the Interval.
        //
        // Returns:
        //     true if the other interval is contained within the limits of this Interval; otherwise
        //     false.
        public bool IncludesInterval(Interval interval, bool strict);
        //
        // Summary:
        //     Tests a parameter for Interval inclusion.
        //
        // Parameters:
        //   t:
        //     Parameter to test.
        //
        // Returns:
        //     true if t is contained within or is coincident with the limits of this Interval.
        public bool IncludesParameter(double t);
        //
        // Summary:
        //     Tests a parameter for Interval inclusion.
        //
        // Parameters:
        //   t:
        //     Parameter to test.
        //
        //   strict:
        //     If true, the parameter must be fully on the inside of the Interval.
        //
        // Returns:
        //     true if t is contained within the limits of this Interval.
        public bool IncludesParameter(double t, bool strict);
        //
        // Summary:
        //     Ensures this Rhino.Geometry.Interval is either singleton or increasing.
        public void MakeIncreasing();
        //
        // Summary:
        //     Converts interval value, or pair of values, to normalized parameter.
        //
        // Returns:
        //     Normalized parameter x so that min*(1.0-x) + max*x = intervalParameter.
        public Interval NormalizedIntervalAt(Interval intervalParameter);
        //
        // Summary:
        //     Converts interval value, or pair of values, to normalized parameter.
        //
        // Returns:
        //     Normalized parameter x so that min*(1.0-x) + max*x = intervalParameter.
        public double NormalizedParameterAt(double intervalParameter);
        //
        // Summary:
        //     Converts normalized parameter to interval value, or pair of values.
        //
        // Returns:
        //     Interval parameter min*(1.0-normalizedParameter) + max*normalizedParameter.
        public double ParameterAt(double normalizedParameter);
        //
        // Summary:
        //     Converts normalized parameter to interval value, or pair of values.
        //
        // Returns:
        //     Interval parameter min*(1.0-normalizedParameter) + max*normalized_paramete.
        public Interval ParameterIntervalAt(Interval normalizedInterval);
        //
        // Summary:
        //     Changes interval to [-T1, -T0].
        public void Reverse();
        //
        // Summary:
        //     Exchanges T0 and T1.
        public void Swap();
        //
        // Summary:
        //     Returns a string representation of this Rhino.Geometry.Interval.
        //
        // Returns:
        //     A string with T0,T1.
        public override string ToString();

        //
        // Summary:
        //     Shifts an interval by a specific amount (addition).
        //
        // Parameters:
        //   number:
        //     The shifting value.
        //
        //   interval:
        //     The interval to be used as a base.
        //
        // Returns:
        //     A new interval where T0 and T1 are summed with number.
        public static Interval operator +(double number, Interval interval);
        //
        // Summary:
        //     Shifts a Rhino.Geometry.Interval by a specific amount (addition).
        //
        // Parameters:
        //   interval:
        //     The interval to be used as a base.
        //
        //   number:
        //     The shifting value.
        //
        // Returns:
        //     A new interval where T0 and T1 are summed with number.
        public static Interval operator +(Interval interval, double number);
        //
        // Summary:
        //     Shifts an interval by a specific amount (subtraction).
        //
        // Parameters:
        //   number:
        //     The shifting value to subtract from (minuend).
        //
        //   interval:
        //     The interval to be subtracted from (subtrahend).
        //
        // Returns:
        //     A new interval with [number-T0, number-T1].
        public static Interval operator -(double number, Interval interval);
        //
        // Summary:
        //     Shifts an interval by a specific amount (subtraction).
        //
        // Parameters:
        //   interval:
        //     The base interval (minuend).
        //
        //   number:
        //     The shifting value to be subtracted (subtrahend).
        //
        // Returns:
        //     A new interval with [T0-number, T1-number].
        public static Interval operator -(Interval interval, double number);
        //
        // Summary:
        //     Determines whether the two Intervals have equal values.
        //
        // Parameters:
        //   a:
        //     The first interval.
        //
        //   b:
        //     The second interval.
        //
        // Returns:
        //     true if the components of the two intervals are exactly equal; otherwise false.
        public static bool operator ==(Interval a, Interval b);
        //
        // Summary:
        //     Determines whether the two Intervals have different values.
        //
        // Parameters:
        //   a:
        //     The first interval.
        //
        //   b:
        //     The second interval.
        //
        // Returns:
        //     true if the two intervals are different in any value; false if they are equal.
        public static bool operator !=(Interval a, Interval b);
        //
        // Summary:
        //     Determines whether the first specified Rhino.Geometry.Interval comes before (has
        //     inferior sorting value than) the second Interval.
        //     The lower bound has first evaluation priority.
        //
        // Parameters:
        //   a:
        //     First interval.
        //
        //   b:
        //     Second interval.
        //
        // Returns:
        //     true if a[0] is smaller than b[0], or a[0] == b[0] and a[1] is smaller than b[1];
        //     otherwise, false.
        public static bool operator <(Interval a, Interval b);
        //
        // Summary:
        //     Determines whether the first specified Rhino.Geometry.Interval comes after (has
        //     superior sorting value than) the second Interval.
        //     The lower bound has first evaluation priority.
        //
        // Parameters:
        //   a:
        //     First interval.
        //
        //   b:
        //     Second interval.
        //
        // Returns:
        //     true if a[0] is larger than b[0], or a[0] == b[0] and a[1] is larger than b[1];
        //     otherwise, false.
        public static bool operator >(Interval a, Interval b);
        //
        // Summary:
        //     Determines whether the first specified Rhino.Geometry.Interval comes before (has
        //     inferior sorting value than) the second Interval, or is equal to it.
        //     The lower bound has first evaluation priority.
        //
        // Parameters:
        //   a:
        //     First interval.
        //
        //   b:
        //     Second interval.
        //
        // Returns:
        //     true if a[0] is smaller than b[0], or a[0] == b[0] and a[1] is smaller than or
        //     equal to b[1]; otherwise, false.
        public static bool operator <=(Interval a, Interval b);
        //
        // Summary:
        //     Determines whether the first specified Rhino.Geometry.Interval comes after (has
        //     superior sorting value than) the second Interval, or is equal to it.
        //     The lower bound has first evaluation priority.
        //
        // Parameters:
        //   a:
        //     First interval.
        //
        //   b:
        //     Second interval.
        //
        // Returns:
        //     true if a[0] is larger than b[0], or a[0] == b[0] and a[1] is larger than or
        //     equal to b[1]; otherwise, false.
        public static bool operator >=(Interval a, Interval b);
    }
}