using System.Numerics;

public static class MathUtilities
{
    public static bool DistanceLessThan(Vector3 point1, Vector3 point2, float distance)
    {
        return (point1 - point2).LengthSquared() < distance * distance;
    }

    public static bool DistanceGreaterThan(Vector3 point1, Vector3 point2, float distance)
    {
        return (point1 - point2).LengthSquared() > distance * distance;
    }
}