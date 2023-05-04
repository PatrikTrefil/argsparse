using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Argparse;

/// <summary>
/// Models a <c>Parser</c> of any context.
/// </summary>
public interface IParser
{
    /// <value>The parser name as it will appear in help and debug messages. The names must not start with "-" or "--".
    /// There must be at least one name.</value>
    public string[] Names { get; init; }
    /// <value>The parser description as it will appear in help.</value>
    public string Description { get; init; }
    public string PlainArgumentsDelimiter { get; init; }
    public ReadOnlyDictionary<string, IParser> SubParsers { get; }
    /// <summary>
    /// Parses command line-like input from <paramref name="args"/> and then invoke
    /// the action provided to the specific parser in <c>Run</c>.
    /// </summary>
    /// <exception cref="InvalidParserConfigurationException">Thrown when the parser is not configured properly.</exception>
    /// <exception cref="ParserConversionException">Thrown when the parser fails to convert a value from string to the intended type.</exception>
    /// <exception cref="ParserRuntimeException">Thrown when the parser fails to run the action associated with an option, flag or argument or a parser.</exception>
    void ParseAndRun(IEnumerable<string> args);

    /// <summary>
    /// Parses command line-like input from <paramref name="args"/>.
    /// </summary>
    /// <exception cref="InvalidParserConfigurationException">Thrown when the parser is not configured properly.</exception>
    /// <exception cref="ParserConversionException">Thrown when the parser fails to convert a value from string to the intended type.</exception>
    /// <exception cref="ParserRuntimeException">Thrown when the parser fails to run the action associated with an option, flag or argument or a parser.</exception>
    void Parse(IEnumerable<string> args);
    /// <summary>
    /// Run the parser. Invoking this methods starts the parsing process.
    /// If there are subparsers configured,
    /// this method should only be invoked on the one selected parsers.
    /// The Run method is invoked after the parsing process is finished.
    /// </summary>
    /// <param name="args">Command line arguments without command strings</param>
    /// <exception cref="InvalidParserConfigurationException">If there is no config instance nor factory</exception>
    /// <exception cref="ParserRuntimeException">On invalid input</exception>
    internal void ParseWithoutCommandsAndRun(IEnumerable<string> args);
    /// <summary>
    /// Run the parser. Invoking this methods starts the parsing process.
    /// If there are subparsers configured,
    /// this method should only be invoked on the one selected parsers
    /// </summary>
    /// <param name="args">Command line arguments without command strings</param>
    /// <exception cref="InvalidParserConfigurationException">If there is no config instance nor factory</exception>
    /// <exception cref="ParserRuntimeException">On invalid input</exception>
    internal void ParseWithoutCommands(IEnumerable<string> args);
}
