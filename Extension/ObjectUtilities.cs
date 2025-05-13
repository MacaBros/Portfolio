using System;
using System.Reflection;

public static class ObjectUtilities
{
    public static void CopyPropertiesAndFields<T>(T source, T destination)
    {
        if (source == null || destination == null)
        {
            throw new ArgumentNullException("Source or destination cannot be null.");
        }

        Type type = typeof(T);
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        foreach (var property in type.GetProperties(flags))
        {
            if (property.CanWrite)
            {
                try
                {
                    property.SetValue(destination, property.GetValue(source));
                }
                catch { }
            }
        }

        foreach (var field in type.GetFields(flags))
        {
            field.SetValue(destination, field.GetValue(source));
        }
    }
}