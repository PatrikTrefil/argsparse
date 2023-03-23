using System;

namespace Argparse;

/// <summary>
/// Represent a flag, an option that is always only either ON or OFF, i.e. without any further
/// value provided by the user.
/// </summary>
/// <typeparam name="C">The cofiguration context type for the given parser, see <see cref="Parser{C}"/> </typeparam>
public sealed record class Flag<C>
{
    /// <summary>
    /// Names the options as they will be parsed by the parser.
    /// Names of long options are prefixed with two dashes '--'
    /// Names of short options are prefixed with one dash '-'
    /// One option can represent both long and short options
    /// </summary>
    public required string[] Names { get; set; }
    /// <summary>
    /// Description of the flag-like option as it should appear in help write-up.
    /// </summary>
    public required string Description { get; set; }
    /// <summary>
    /// Action to be carried out upon parsing of the flag in the input.
    /// </summary>
    public required Action<C> Action { get; set; }
}
