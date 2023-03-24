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

     public static Func<string, double> CreateDoubleConverter(double minValueInclusive = double.NegativeInfinity, double maxValueInclusive = double.PositiveInfinity) {
        return null!;
     }

    /// <summary>
    /// Creates a converter for an enumeration type.
    /// </summary>
    /// <typeparam name="T">The enumeration type, has to be Enum</typeparam>
    /// <param name="caseSensitive">If true, the converter match enum values no matter the case</param>
    /// <param name="explicitMapping">A dictionary that maps strings to enum values. If a string is not found in the dictionary, the converter will try to parse the string as an enum value. If no mapping is provided, the convertor will try to match enum names.</param>
    /// <param name="useImplicitMapping">If true, the converter will not try to match the input string to enum value names in code, but only use provided explicit mapping.</param>
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
        bool caseSensitive = true, Dictionary<string, T>? explicitMapping = null, bool useImplicitMapping = true)
        where T : Enum
    {        
        return null!;
    }

    /// <summary>
    /// Creates a converter for an enumeration type.
    /// </summary>
    /// <typeparam name="T">The enumeration type, has to be Enum</typeparam>
    /// <param name="caseSensitive">If true, the converter match enum values no matter the case</param>
    /// <param name="mapping">A dictionary that maps strings to enum values. If a string is not found in the dictionary, the converter will try to parse the string as an enum value. If no mapping is provided, the convertor will try to match enum names.</param>
    /// <param name="disableImplicitMapping">If true, the converter will not try to match the input string to enum value names in code, but only use provided explicit mapping.</param>
    /// <description>
    /// The converter will try to match the input string to the enum values in the following order:
    /// 1. Explicit mapping: the converter will try to match the input string to the keys in the <paramref cref="explicitMapping">.
    /// 2. Implicit mapping: the converter will try to match the input string to the keys in the mapping.
    /// 3. If not found in any mapping, <see cref="ArgumentException"/> will be thrown.
    /// <para>
    /// Implicit mapping can be disabled by setting <paramref cref="useImplicitMapping"/> to false.
    /// Explicit mapping can be disabled leaving out <paramref cref="explicitMapping"/> or by setting it to null .
    /// </para>
    /// <para>
    /// If a string is present in more than one key array, the first match will be used.
    /// </para>
    /// </description>
    public static Func<string, T> CreateEnumerationConverter<T>(bool caseSensitive = true, Dictionary<string[], T>? explicitMapping = null, bool useImplicitMapping = true)
        where T : Enum
    {        
        return null!;
    }
}
