using System;
using System.Numerics;

public static class TransformUtilities
{
    public static void ResetTransform(dynamic obj)
    {
        obj.Position = Vector3.Zero;
        obj.Rotation = Quaternion.Identity;
        obj.Scale = Vector3.One;
    }
}