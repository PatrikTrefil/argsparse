using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Argparse;

public interface IOption<C>
{
    /// <summary>
    /// Names the options as they will be parsed by the parser.
    /// Names of long options are prefixed with two dashes '--'
    /// Names of short options are prefixed with one dash '-'
    /// One option can represent both long and short options
    /// </summary>
    public string[] Names { get; init; }

    /// <summary>
    /// Description of the option as it should appear in help write-up.
    /// </summary>
    public string Description { get; init; }
    /// <summary>
    /// If true, the parser will produce an error state upon finishing parsing without encountering
    /// the option.
    /// </summary>
    public bool IsRequired { get; init; }
    /// <summary>
    /// Value used in help message as placeholder for value.
    /// e.g. when set to FILE for option --output, the help
    /// message will be: --output=FILE
    /// </summary>
    public string ValuePlaceHolder { get; init; }
    internal void Process(C config, string value);
}

/// <summary>
/// Models a parser option which is to accept a value provided by the user.
/// </summary>
/// <typeparam name="C">The cofiguration context type for the given parser, see <see cref="Parser{C}"/></typeparam>
/// <typeparam name="V">Type of the value the provided string is to be converted to. /// </typeparam>
public sealed partial record class Option<C, V> : IOption<C>
{
    [GeneratedRegex("(^--[a-zA-Z1-9]+[a-zA-Z1-9-]*$)|(^-[a-zA-Z]$)")]
    private static partial Regex LongOrShortName();

    private readonly string[] names;
    /// <summary>Array of names of the option. It must be set to a non-empty array.</summary>
    /// <exception cref="ArgumentException">Thrown when set to an empty array or the any of the
    /// provided names is in an invalid format.</exception>
    public required string[] Names
    {
        get => names; init
        {
            if (value.Length == 0)
                throw new ArgumentException("Option must have at least one name");

            var invalidOptionNames = value.Where(name => !LongOrShortName().IsMatch(name));
            if (invalidOptionNames.Any())
                throw new ArgumentException($"Invalid option names: {string.Join(", ", invalidOptionNames)}");

            names = value;
        }
    }
    public string ValuePlaceHolder { get; init; } = "<value>";
    public required string Description { get; init; }
    /// <summary>
    /// Action to be carried out upon parsing and conversion of the option from the input.
    /// </summary>
    public required Action<C, V> Action { get; init; }
    public bool IsRequired { get; init; } = false;
    /// <summary>
    /// Function to convert the option value parsed as a string from the input to the target type.
    /// </summary>
    /// <remarks>
    /// <para>Converters may throw exceptions, 
    /// they will be caught and the parser will throw <see cref="ParserConversionException"/></para>
    /// <para>Some of the basic converters are available in <see cref="ConverterFactory"/></para>
    /// <para>It is good practice to not use converters to store the value or manipulate outer state.
    /// </para>
    /// </remarks>
    public required Func<string, V> Converter { get; init; }
    void IOption<C>.Process(C config, string value)
    {
        V convertedValue;
        try
        {
            convertedValue = Converter(value);
        }
        catch (Exception ex)
        {
            throw new ParserConversionException($"Value conversion for option {Names[0]} failed", ex);
        }
        Action(config, convertedValue);
    }
}
