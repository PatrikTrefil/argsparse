using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Argparse;

public static class ConverterFactory
{

    /// <summary>
    /// Creates a converter that does not convert the input string, but returns it as is.
    /// </summary>
    public static Func<string, string> CreateStringConverter() => (string s) => s;

    /// <summary>
    /// Creates a converter that converts a string to a list of values of type T.
    /// </summary>
    /// <typeparam name="T">Type of each element in the list</typeparam>
    /// <param name="converter">Converter to be used for indivial elements, after they are parsed from their list representations.</param>
    /// <param name="separator">Separator that should separate the individual values. May not be whitespace.</param>
    /// <exception cref="ArgumentException">If the separator is a whitespace.</exception>
    public static Func<string, List<T>> CreateListConverter<T>(Func<string, T> converter, char separator = ',')
    {
        return (string s) => { return s.Split(separator).Select(x => converter(x)).ToList(); };
    }

    private static string[] trueValues = new string[] { "true", "1", "yes", "y", "on" };
    private static string[] falseValues = new string[] { "false", "0", "no", "n", "off" };
    /// <summary>
    /// Creates a converter that converts a value to boolean.
    /// Accepts following values: true, false, 1, 0, yes, no, y, n, on, off.
    /// Created converter is case insensitive.
    /// </summary>
    public static Func<string, bool> CreateBoolConverter()
    {
        // this implementation is only a demo, it is not complete, do not base tests on it
        return (string s) =>
            {
                string val = s.ToLower();
                if (trueValues.Contains(val))
                    return true;
                if (falseValues.Contains(val))
                    return false;
                throw new ParserConversionException($"Value {s} is not a valid boolean value.");
            };
    }

    /// <summary>
    /// Creates a converter that converts a value to integer.
    /// </summary>
    /// <param name="minValueInclusive">
    /// Minimum value that the converter will accept. If the value is lower, <see cref="ArgumentException"/> will be thrown.
    /// </param>
    /// <param name="maxValueInclusive">
    /// Maximum value that the converter will accept. If the value is higher, <see cref="ArgumentException"/> will be thrown.
    /// </param>
    /// <exception cref="ArgumentException">
    /// If the min value provided is higher than the max value.
    /// </exception>
    public static Func<string, int> CreateIntConverter(int minValueInclusive = int.MinValue, int maxValueInclusive = int.MaxValue)
    {
        return (string s) =>
            {
                int value;
                bool result = int.TryParse(s, out value);
                if (!result)
                    throw new ParserConversionException($"Value {s} is not a valid integer value.");
                if (value < minValueInclusive || value > maxValueInclusive)
                    throw new ParserConversionException(
                        $"Value {value} is out of range [{minValueInclusive}, {maxValueInclusive}]"
                        );
                return value;
            };
    }


    /// <summary>
    /// Creates a converter that converts a value to a double.
    /// </summary>
    /// <param name="minValueInclusive">
    /// Minimum value that the converter will accept. If the value is lower, <see cref="ArgumentException"/> will be thrown.
    /// </param>
    /// <param name="maxValueInclusive">
    /// Maximum value that the converter will accept. If the value is higher, <see cref="ArgumentException"/> will be thrown.
    /// </param>
    /// <exception cref="ArgumentException">
    /// If the min value provided is higher than the max value.
    /// </exception>
    public static Func<string, double> CreateDoubleConverter(double minValueInclusive = double.NegativeInfinity, double maxValueInclusive = double.PositiveInfinity)
    {
        return (string s) =>
        {
            double value;
            bool result = double.TryParse(s, out value);
            if (!result)
                throw new ParserConversionException($"Value {s} is not a valid floating point value.");
            if (value < minValueInclusive || value > maxValueInclusive)
                throw new ParserConversionException(
                    $"Value {value} is out of range [{minValueInclusive}, {maxValueInclusive}]"
                    );
            return value;
        };
    }

    /// <summary>
    /// Creates a converter for an enumeration type.
    /// </summary>
    /// <typeparam name="T">The enumeration type, has to be Enum</typeparam>
    /// <param name="caseSensitive">If true, the converter match enum values no matter the case</param>
    /// <param name="explicitMapping">A dictionary that maps enum values to possible string values provided by the user. If a string is not found in the dictionary values, the converter will try to parse the string as an enum value. If no explicit mapping is provided, the convertor will use implicit mapping</param>
    /// <param name="useImplicitMapping">If true, the converter will not try to match the input string to enum value names in code, but only use provided explicit mapping.</param>
    /// <exception cref="ArgumentException">If both explicit and implicit mapping are disabled.</exception>
    /// <description>
    /// The converter will try to match the input string to the enum values in the following order:
    /// 1. Explicit mapping: the converter will try to match the input string to the keys in the <paramref cref="explicitMapping">.
    /// 2. Implicit mapping: the converter will try to match the input string to the keys in the mapping.
    /// 3. If not found in any mapping, <see cref="ArgumentException"/> will be thrown.
    /// <para>
    /// Implicit mapping can be disabled by setting <paramref cref="useImplicitMapping"/> to false.
    /// Explicit mapping can be disabled leaving out <paramref cref="explicitMapping"/> or by setting it to null .
    /// </para>
    /// </description>
    public static Func<string, T> CreateEnumerationConverter<T>(
        bool caseSensitive = true, Dictionary<T, ImmutableHashSet<string>>? explicitMapping = null, bool useImplicitMapping = true)
        where T : Enum
    {
        return null!;
    }

}
