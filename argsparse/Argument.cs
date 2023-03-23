using System;
using System.Collections.Generic;

namespace Argparse;

/// <summary>
/// Models information about a plain argument, how many parts it should consist of.
/// <para>
/// See <see cref="Argument{C, V}"/>, <see cref="IArgument{C}"/>.
/// </para>
/// </summary>
public abstract record class ArgumentMultiplicity
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
    /// <summary>
    /// Hiding the constructor so that the user can't create a class that is derived from this one.
    /// </summary>
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
    /// This action is run for every value parsed from the input, e.g. when the argument is specified
    /// to have 3 values and is required, this action will be run exactly 3 times.
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
/// A convenience class used for speedy creation of common argument types
/// for a <see cref="Parser{C}"/> with config context <typeparamref name="C"/>.
/// </summary>
public static class ArgumentFactory<C>
{
    /// <summary>
    /// Creates a simple string-valued argument for a <see cref="Parser{C}"/> 
    /// with config context <typeparamref name="C"/>.
    /// </summary>
    public static Argument<C, string> CreateStringArgument()
    {
        return new Argument<C, string>() { Converter = ConverterFactory.CreateStringConverter() };
    }

    /// <summary>
    /// Creates a argument which accepts a single string which is a list of values convertible to type <typeparamref name="T"/>
    /// by the provided convertor function <paramref name="converter"/>.
    /// </summary>
    /// <typeparam name="C">Config context type of the parent parser.</typeparam>
    /// <typeparam name="T">Type of the individual values listed in the argument value.</typeparam>
    /// <param name="converter">Function to convert the individiual values from string to intended type.</param>
    /// <param name="separator">Separator of the individual values in the argument value</param>
    public static Argument<C, List<T>> CreateListArgument<T>(Func<string, T> converter, char separator = ',')
    {
        return new Argument<C, List<T>>() { Converter = ConverterFactory.CreateListConverter(converter, separator) };
    }
    /// <summary>
    /// Creates a simple bool-valued argument for a <see cref="Parser{C}"/> 
    /// with config context <typeparamref name="C"/>. The argument value will be true
    /// if the string value is "true" and false otherwise.
    /// </summary>
    public static Argument<C, bool> CreateBoolArgument()
    {
        return new Argument<C, bool>()
        {
            Converter = ConverterFactory.CreateBoolConverter()
        };
    }
    /// <summary>
    /// Creates a simple int-valued argument for a <see cref="Parser{C}"/> 
    /// with config context <typeparamref name="C"/>. You can limit the range of valid
    /// values by providing <paramref name="minValue"/> and <paramref name="maxValue"/>.
    /// If the converted value is out of range, an <see cref="ArgumentException"/> will be thrown.
    /// If the parsing of the string value fails, a <see cref="FormatException"/> will be thrown.
    /// </summary>
    public static Argument<C, int> CreateIntArgument(int minValue = int.MinValue, int maxValue = int.MaxValue)
    {
        return new Argument<C, int>()
        {
            Converter = ConverterFactory.CreateIntConverter(minValue, maxValue)
        };
    }
}
