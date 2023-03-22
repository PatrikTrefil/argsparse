using System;
using System.Collections.Generic;
using System.Linq;

namespace Argsparse;

public interface IOption<C>
{
    /// <summary>
    /// Names the options as they will be parsed by the parser.
    /// Names of long options are prefixed with two dashes '--'
    /// Names of short options are prefixed with one dash '-'
    /// One option can represent both long and short options
    /// </summary>
    public string[]? Names { get; set; }

    /// <summary>
    /// Description of the option as it should appear in help write-up.
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// If false, the parser will produce an error state upon finishing parsing without encountering
    /// the option.
    /// </summary>
    public bool IsRequired { get; set; }
    /// <summary>
    /// Value used in help message as placeholder for value.
    /// e.g. when set to FILE for option --output, the help
    /// message will be: --output=FILE
    /// </summary>
    public string ValuePlaceHolder { get; set; }
    internal void Process(C config, string value);
}

/// <summary>
/// Models a parser option which is to accept a value provided by the user.
/// </summary>
/// <typeparam name="C">The cofiguration context type for the given parser, see <see cref="Parser{C}"/></typeparam>
/// <typeparam name="V">Type of the value the provided string is to be converted to. /// </typeparam>
public sealed record class Option<C, V> : IOption<C>
{
    public string[]? Names { get; set; }
    public string ValuePlaceHolder { get; set; } = "<value>";
    public string? Description { get; set; }
    /// <summary>
    /// Action to be carried out upon parsing and conversion of the option from the input.
    /// </summary>
    public Action<C, V> Action { get; set; } = (conf, val) => { };
    public bool IsRequired { get; set; } = false;
    /// <summary>
    /// Function to convert the argument value parsed as a string from the input to the target type.
    /// </summary>
    public Func<string, V> Converter { get; set; } = (strVal) => default;
    void IOption<C>.Process(C config, string value)
    {
        V convertedValue = Converter(value);
        Action(config, convertedValue);
    }
}

/// <summary>
/// A convenience class used for speedy creation of common option types.
/// </summary>
public static class OptionFactory
{
    /// <summary>
    /// Creates a simple string-valued option for a <see cref="Parser{C}"/> 
    /// with config context <typeparamref name="C"/>.
    /// </summary>
    public static Option<C, string> CreateStringOption<C>()
    {
        return new Option<C, string>() { Converter = (string s) => { return s; } };
    }

    /// <summary>
    /// Creates a option which accepts a list of values convertible to type <typeparamref name="T"/>
    /// by the provided convertor function <paramref name="convertor"/>.
    /// </summary>
    /// <typeparam name="C">Config context type of the parent parser.</typeparam>
    /// <typeparam name="T">Type of the individual values listed in the option value.</typeparam>
    /// <param name="convertor">Function to convert the individiual values from string to intended type.</param>
    /// <param name="separator">Separator of the individual values in the option value</param>
    public static Option<C, List<T>> CreateListOption<C, T>(Func<string, T> convertor, char separator = ',')
    {
        return new Option<C, List<T>>() { Converter = (string s) => { return s.Split(separator).Select(x => convertor(x)).ToList(); } };
    }
    /// <summary>
    /// Creates a simple bool-valued option for a <see cref="Parser{C}"/> 
    /// with config context <typeparamref name="C"/>. The option will be true if the
    /// string value is "true" and false otherwise.
    /// </summary>
    public static Option<C, bool> CreateBoolOption<C>()
    {
        return new Option<C, bool>()
        {
            Converter = (string s) =>
            {
                if (s == "true") return true;
                return false;
            }
        };
    }
    /// <summary>
    /// Creates a simple int-valued option for a <see cref="Parser{C}"/> 
    /// with config context <typeparamref name="C"/>. You can limit the range of valid
    /// values by providing <paramref name="minValue"/> and <paramref name="maxValue"/>.
    /// If the converted value is out of range, an <see cref="ArgumentException"/> will be thrown.
    /// If the parsing of the string value fails, a <see cref="FormatException"/> will be thrown.
    /// </summary>
    public static Option<C, int> CreateIntOption<C>(int minValue = int.MinValue, int maxValue = int.MaxValue)
    {
        var intOption = new Option<C, int>();
        intOption = intOption with
        {
            Converter = (string s) =>
            {
                int value = int.Parse(s);
                if (value < minValue || value > maxValue)
                {
                    string optionName = intOption.Names is null ? string.Join(", ", intOption.Names) : "no-name-provided";
                    throw new ArgumentException($"Value {value} in option named {optionName} is out of range [{minValue}, {maxValue}]");
                }

                return value;
            }
        };
        return intOption;
    }
}
