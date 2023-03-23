using System;
using System.Collections.Generic;
using System.Linq;

namespace Argparse;

public static class ConverterFactory
{
    public static Func<string, string> CreateStringConverter() => (string s) => { return s; };
    public static Func<string, List<T>> CreateListConverter<T>(Func<string, T> converter, char separator = ',')
    {
        return (string s) => { return s.Split(separator).Select(x => converter(x)).ToList(); };
    }
    public static Func<string, bool> CreateBoolConverter()
    {
        // this implementation is only a demo, it is not complete, do not base tests on it
        return (string s) =>
            {
                if (s == "true") return true;
                // more logic here
                return false;
            };
    }
    public static Func<string, int> CreateIntConverter(int minValueInclusive = int.MinValue, int maxValueInclusive = int.MaxValue)
    {
        // this implementation is only a demo, it is not complete, do not base tests on it
        return (string s) =>
            {
                int value = int.Parse(s);
                if (value <= minValueInclusive || value >= maxValueInclusive)
                    throw new ArgumentException(
                        $"Value {value} is out of range [{minValueInclusive}, {maxValueInclusive}]"
                        );

                return value;
            };
    }
}
