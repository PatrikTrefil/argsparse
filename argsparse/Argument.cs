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
    /// Description of the argument as it should appear in help write-up.
    /// </summary>
    public string Description { get; init; }
    /// <summary>
    /// Value placeholder will be used in synopsis e.g. program [options] <value-placeholder> and appear in help write-up.
    /// </summary>
    public string ValuePlaceholder { get; init; }
    /// <summary>
    /// Codifies information about what number or range of numbers of parts for this argument is to be expected.
    /// </summary>
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
    public required Func<string, V> Converter { get; init; }
    /// <summary>
    /// This action is run for every value parsed from the input, e.g. when the argument is specified
    /// to have 3 values and is required, this action will be run exactly 3 times.
    /// </summary>
    public required Action<C, V> Action { get; init; }

    public ArgumentMultiplicity Multiplicity { get; init; } = new ArgumentMultiplicity.SpecificCount(Number: 1, IsRequired: true);
    void IArgument<C>.Process(C config, string value)
    {
        V convertedValue = Converter(value);
        Action(config, convertedValue);
    }
}
