using System;

internal static class StringArray
{
    public static void Deconstruct<T>(this T[] array, out T o0)
    {
        if (array == null || array.Length < 1)
            throw new ArgumentException("Cannot unpack array");

        o0 = array[0];
    }

    public static void Deconstruct<T>(this T[] array, out T o0, out T o1)
    {
        if (array == null || array.Length < 2)
            throw new ArgumentException("Cannot unpack array");

        o0 = array[0];
        o1 = array[1];
    }

    public static void Deconstruct<T>(this T[] array, out T o0, out T o1, out T o2)
    {
        if (array == null || array.Length < 3)
            throw new ArgumentException("Cannot unpack array");

        o0 = array[0];
        o1 = array[1];
        o2 = array[2];
    }

}
