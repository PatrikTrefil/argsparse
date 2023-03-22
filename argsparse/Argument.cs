using System;
using System.Collections.Generic;
using System.Linq;

namespace Argsparse;

/// <summary>
/// Models information about a plain argument, how many parts it should consist of.
/// <para>
/// See <see cref="Argument{C, V}"/>, <see cref="IArgument{C}"/>.
/// </para>
/// </summary>
public record class ArgumentMultiplicity
{
    /// <summary>
    /// Represents such a multiplicity in which the number of parts and argument should have is one
    /// number known in advance.
    /// </summary>
    /// <param name="IsRequired">Allows for the omitting of the argument.</param>
    public sealed record class SpecificCount(int Number, bool IsRequired) : ArgumentMultiplicity();
    /// <summary>
    /// Represents such a multiplicity in which all the following argument parts belong to the given
    /// plain argument. 
    /// </summary>
    /// <param name="MinimumNumberOfArguments">Allow for the specification of minimum number of parts required
    /// which should then be enforced by the parser.</param>
    public sealed record class AllThatFollow(int MinimumNumberOfArguments = 0) : ArgumentMultiplicity();
    private ArgumentMultiplicity() { }
}

public interface IArgument<C>
{
    /// <summary>
    /// Name of the argument as it should appear in help write-up and debug messages.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Description of the argument as it should appear in help write-up.
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// Value placeholder will be used in synopsis e.g. program [options] <value-placeholder> ...
    /// </summary>
    public string ValuePlaceholder { get; set; }
    /// <summary>
    /// Codifies information about what number or range of numbers of parts for this argument is to be expected.
    /// </summary>
    public ArgumentMultiplicity Multiplicity { get; set; }
    internal void Process(C config, string value);
}

public sealed record class Argument<C, V> : IArgument<C>
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string ValuePlaceholder { get; set; } = "<arg>";
    /// <summary>
    /// Function to convert the argument value parsed as a string from the input to the target type.
    /// </summary>
    public Func<string, V> Converter { get; set; } = (strVal) => default;
    /// <summary>
    /// Action to be carried out upon parsing of the argument in the input.
    /// </summary>
    public Action<C, V> Action { get; set; } = (conf, val) => { };

    public ArgumentMultiplicity Multiplicity { get; set; } = new ArgumentMultiplicity.SpecificCount(Number: 1, IsRequired: true);
    void IArgument<C>.Process(C config, string value)
    {
        V convertedValue = Converter(value);
        Action(config, convertedValue);
    }
}

/// <summary>
/// A convenience class used for speedy creation of common argument types.
/// </summary>
public static class ArgumentFactory
{
    /// <summary>
    /// Creates a simple string-valued argument for a <see cref="Parser{C}"/> 
    /// with config context <typeparamref name="C"/>.
    /// </summary>
    public static Argument<C, string> CreateStringArgument<C>()
    {
        return new Argument<C, string>() { Converter = (string s) => { return s; } };
    }

    /// <summary>
    /// Creates a argument which accepts a list of values convertible to type <typeparamref name="T"/>
    /// by the provided convertor function <paramref name="convertor"/>.
    /// </summary>
    /// <typeparam name="C">Config context type of the parent parser.</typeparam>
    /// <typeparam name="T">Type of the individual values listed in the argument value.</typeparam>
    /// <param name="convertor">Function to convert the individiual values from string to intended type.</param>
    /// <param name="separator">Separator of the individual values in the argument value</param>
    public static Argument<C, List<T>> CreateListArgument<C, T>(Func<string, T> convertor, char separator = ',')
    {
        return new Argument<C, List<T>>() { Converter = (string s) => { return s.Split(separator).Select(x => convertor(x)).ToList(); } };
    }
    /// <summary>
    /// Creates a simple bool-valued argument for a <see cref="Parser{C}"/> 
    /// with config context <typeparamref name="C"/>.
    /// </summary>
    public static Argument<C, bool> CreateBoolArgument<C>()
    {
        return new Argument<C, bool>()
        {
            Converter = (string s) =>
            {
                if (s == "true") return true;
                return false;
            }
        };
    }
    /// <summary>
    /// Creates a simple int-valued argument for a <see cref="Parser{C}"/> 
    /// with config context <typeparamref name="C"/>. You can limit the range of valid
    /// values by providing <paramref name="minValue"/> and <paramref name="maxValue"/>.
    /// If the converted value is out of range, an <see cref="ArgumentException"/> will be thrown.
    /// If the parsing of the string value fails, a <see cref="FormatException"/> will be thrown.
    /// </summary>
    public static Argument<C, int> CreateIntArgument<C>(int minValue = int.MinValue, int maxValue = int.MaxValue)
    {
        var intArgument = new Argument<C, int>();
        intArgument = intArgument with
        {
            Converter = (string s) =>
            {
                int value = int.Parse(s);
                if (value < minValue || value > maxValue)
                {
                    string argName = intArgument.Name ?? "no-name-provided";
                    throw new ArgumentException($"Value {value} in argument named {argName} is out of range [{minValue}, {maxValue}]");
                }

                return value;
            }
        };
        return intArgument;
    }
}