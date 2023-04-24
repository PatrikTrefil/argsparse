using System;

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
    /// <remarks>Only one non-required argument is allowed per parser. 
    /// Do not mix non-required arguments with <see cref="AllThatFollow"/></remarks>
    public sealed record class SpecificCount : ArgumentMultiplicity
    {
        public int Number { get; }
        public bool IsRequired { get; }

        /// <exception cref="ArgumentException">Thrown when <paramref name="Number"/> is not positive.</exception>
        public SpecificCount(int Number, bool IsRequired)
        {
            if (Number <= 0)
                throw new ArgumentException("Count must be positive.");

            this.Number = Number;
            this.IsRequired = IsRequired;
        }
    }
    /// <summary>
    /// Represents such a multiplicity in which all the following argument parts belong to the given
    /// plain argument. 
    /// </summary>
    /// <param name="MinimumNumberOfArguments">Allow for the specification of minimum number of parts required
    /// which should then be enforced by the parser.</param>
    /// <remarks>Only one such argument is allowed per parser. 
    /// Do not mix non-required <see cref="SpecificCount"/> arguments with <see cref="AllThatFollow"/></remarks>
    public sealed record class AllThatFollow : ArgumentMultiplicity
    {
        public int MinimumNumberOfArguments { get; }
        /// <param name="MinimumNumberOfArguments">Minimum number of values that should be present for the argument to be considered valid. This number must be non-negative.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="MinimumNumberOfArguments"/> is negative.</exception>
        public AllThatFollow(int MinimumNumberOfArguments = 0)
        {
            if (MinimumNumberOfArguments < 0)
                throw new ArgumentException("Minimum number of arguments must be non-negative");

            this.MinimumNumberOfArguments = MinimumNumberOfArguments;
        }
    }

    /// <summary>
    /// Hiding the constructor so that the user can't create a class that is derived from this one.
    /// </summary>
    private ArgumentMultiplicity() { }
}

public interface IArgument<C>
{

    /// <summary>
    /// Description of the argument as it should appear in help write-up.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Value placeholder will be used in synopsis and appear in help write-up, 
    /// e.g. program [options] <value-placeholder>
    /// ... 
    /// value-placeholder   Description of the argument.
    /// </summary>
    public string ValuePlaceholder { get; init; }
    /// <summary>
    /// Codifies information about what number or range of numbers of parts for this argument is to be expected.
    /// <para>
    /// Defaults to <see cref="ArgumentMultiplicity.SpecificCount"/> with <see cref="ArgumentMultiplicity.SpecificCount.Number"/> set to 1 and
    /// <see cref="ArgumentMultiplicity.SpecificCount.IsRequired"/> set to true.
    /// </para>
    /// </summary>
    /// <remarks>Only one  <see cref="AllThatFollow"/> argument is allowed per parser. 
    /// Only one non-required argument is allowed per parser.
    /// Do not mix non-required <see cref="SpecificCount"/> arguments with <see cref="AllThatFollow"/></remarks>
    public ArgumentMultiplicity Multiplicity { get; init; }
    internal void Process(C config, string value);
}

public sealed record class Argument<C, V> : IArgument<C>
{
    public required string Description { get; init; }
    public string ValuePlaceholder { get; init; } = "<arg>";
    /// <summary>
    /// Function to convert the argument value parsed as a string from the input to the target type.
    /// </summary>
    /// <remarks>
    /// <para>Converters may throw exceptions, 
    /// they will be caught and the parser will throw <see cref="ParserConversionException"/></para>
    /// <para>Some of the basic converters are available in <see cref="ConverterFactory"/></para>
    /// <para>It is good practice to not use converters to store the value or manipulate outer state.
    /// </para>
    /// </remarks>
    public required Func<string, V> Converter { get; init; }
    /// <summary>
    /// This action is run for every value parsed from the input, e.g. when the argument is specified
    /// to have 3 values and is required, this action will be run exactly 3 times.
    /// </summary>
    /// <value>Converters may throw exceptions, 
    /// they will be caught and the parser will throw <see cref="ParserConversionException"/></value>
    public required Action<C, V> Action { get; init; }

    public ArgumentMultiplicity Multiplicity { get; init; } = new ArgumentMultiplicity.SpecificCount(Number: 1, IsRequired: true);
    /// <summary>
    /// Process is called when the argument is encountered in the input.
    /// </summary>
    void IArgument<C>.Process(C config, string value)
    {
        V convertedValue = Converter(value);
        Action(config, convertedValue);
    }
}
