using System;
using System.Numerics;

public static class QuaternionUtilities
{
    public static float GetSignedAngle(Quaternion a, Quaternion b, Vector3 axis)
    {
        Quaternion delta = Quaternion.Concatenate(b, Quaternion.Inverse(a));
        Vector3 angleAxis = Vector3.Zero;
        float angle = 0;

        if (delta.LengthSquared() > 0)
        {
            angle = 2 * MathF.Acos(delta.W);
            angleAxis = new Vector3(delta.X, delta.Y, delta.Z) / MathF.Sin(angle / 2);
        }

        return Vector3.Dot(angleAxis, axis) < 0 ? -angle : angle;
    }
}