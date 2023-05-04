using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Argparse;

/// <summary>
/// Represent a flag, an option that is always only either ON or OFF, i.e. without any further
/// value provided by the user.
/// </summary>
/// <typeparam name="C">The cofiguration context type for the given parser, see <see cref="Parser{C}"/> </typeparam>
public sealed partial record class Flag<C>
{
    [GeneratedRegex("(^--[a-zA-Z1-9]+[a-zA-Z1-9-]*$)|(^-[a-zA-Z]$)")]
    private static partial Regex LongOrShortName();
    /// <value>
    /// Backing field for <see cref="Names"/>
    /// </value>
    private readonly string[] names;
    /// <summary>
    /// Names the options as they will be parsed by the parser.
    /// Names of long options are prefixed with two dashes '--'
    /// Names of short options are prefixed with one dash '-'
    /// One option can represent both long and short options
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when set to an empty array or the any of the
    /// provided names is in an invalid format.</exception>
    public required string[] Names
    {
        get => names; init
        {
            if (value.Length == 0)
                throw new ArgumentException("Flag must have at least one name");

            var invalidOptionNames = value.Where(name => !LongOrShortName().IsMatch(name));
            if (invalidOptionNames.Any())
                throw new ArgumentException($"Invalid option names: {string.Join(", ", invalidOptionNames)}");

            names = value;
        }
    }
    /// <summary>
    /// Description of the flag-like option as it should appear in help write-up.
    /// </summary>
    public required string Description { get; set; }
    /// <summary>
    /// Action to be carried out upon parsing of the flag in the input.
    /// The action is carried out each time the flag is encountered.
    /// </summary>
    public required Action<C> Action { get; set; }
}
