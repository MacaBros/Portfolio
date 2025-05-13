using System;
using System.Collections.Generic;

public static class MyExtensions
{
    public static T RandomElement<T>(this T[] array, Random rng = null)
    {
        if (array.Length == 0)
        {
            return default;
        }

        rng ??= new Random();
        return array[rng.Next(array.Length)];
    }

    public static T RandomElement<T>(this List<T> list, Random rng = null)
    {
        if (list.Count == 0)
        {
            return default;
        }

        rng ??= new Random();
        return list[rng.Next(list.Count)];
    }

    public static List<T> RemoveAtSwapBack<T>(this List<T> list, int index)
    {
        if (index < 0 || index >= list.Count)
        {
            throw new IndexOutOfRangeException($"{index} < 0 || {index} >= {list.Count}");
        }

        list[index] = list[^1];
        list.RemoveAt(list.Count - 1);

        return list;
    }

    public static T[] ShuffleArray<T>(this T[] array, Random rng = null)
    {
        rng ??= new Random();
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }

        return array;
    }

    public static void ShuffleList<T>(this IList<T> list, Random rng = null)
    {
        rng ??= new Random();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public static string GetFullPath(this object obj, Func<object, object> getParent, Func<object, string> getName)
    {
        string path = $"/{getName(obj)}";

        while ((obj = getParent(obj)) != null)
        {
            path = $"/{getName(obj)}{path}";
        }

        return path;
    }

    public static bool IsInsideBounds(this (float x, float y, float z) point, (float x, float y, float z) center, (float x, float y, float z) size)
    {
        return Math.Abs(point.x - center.x) <= size.x / 2 &&
               Math.Abs(point.y - center.y) <= size.y / 2 &&
               Math.Abs(point.z - center.z) <= size.z / 2;
    }
}