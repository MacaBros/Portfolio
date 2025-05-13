using System;
using System.Collections.Generic;

public static class HierarchyUtilities
{
    public static T CloneHierarchy<T>(T source, Func<T, T> cloneFunc, Func<T, IEnumerable<T>> getChildren, Action<T, T> setParent)
    {
        T clone = cloneFunc(source);
        foreach (var child in getChildren(source))
        {
            T childClone = CloneHierarchy(child, cloneFunc, getChildren, setParent);
            setParent(childClone, clone);
        }
        return clone;
    }
}