using System;
using System.Collections.Generic;
using System.Linq;

namespace Argparse;

internal static class ConverterFactory
{
    public static Func<string, string> CreateStringConverter() => (string s) => { return s; };
    public static Func<string, List<T>> CreateListConverter<T>(Func<string, T> converter, char separator)
    {
        return (string s) => { return s.Split(separator).Select(x => converter(x)).ToList(); };
    }
    public static Func<string, bool> CreateBoolConverter()
    {
        return (string s) =>
            {
                if (s == "true") return true;
                return false;
            };
    }
    public static Func<string, int> CreateIntConverter(int minValue, int maxValue)
    {
        return (string s) =>
            {
                int value = int.Parse(s);
                if (value < minValue || value > maxValue)
                    throw new ArgumentException(
                        $"Value {value} is out of range [{minValue}, {maxValue}]"
                        );

                return value;
            };

    }
}
