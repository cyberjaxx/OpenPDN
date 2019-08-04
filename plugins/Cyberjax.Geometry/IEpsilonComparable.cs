
namespace Cyberjax.Geometry
{
    public interface IEpsilonComparable<in T>
    {
        bool EpsilonEquals(T other, double epsilon);
    }
}
