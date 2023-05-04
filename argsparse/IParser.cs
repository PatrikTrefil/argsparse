using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

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
    /// <value>Used to configure the arguments delimiter,
    /// which when parsed gives signal to the parser to treat all subsequent tokens as plain
    /// arguments.
    /// </value>
    public string PlainArgumentsDelimiter { get; init; }
    /// <summary>
    /// Returns a dictionary of all atached subparsers with keys being names of the 
    /// commands attached to the parsers.
    /// <para>See <see cref="AddSubparser(string, IParser)"</see></para>
    /// </summary>
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
    /// <summary>
    /// Prints a formatted help message to the provided output <paramref name="writer"/> with information
    /// about parser usage, arguments, options, and subcommand parsers.
    /// </summary>
    public void PrintHelp(TextWriter writer);
    /// <summary>
    /// Prints a formatted help message to the console with information
    /// about parser usage, arguments, options, and subcommand parsers.
    /// </summary>
    public void PrintHelp();
}

public interface IParser<C> : IParser
{
    /// <summary>
    /// Prints a formatted help message to the console with information
    /// about parser usage, arguments, options, and subcommand parsers as formatted by the provided <paramref name="formatter"/>.
    /// </summary>
    public void PrintHelp(IParserHelpFormatter<C> formatter) => PrintHelp(formatter, Console.Out);
    /// <summary>
    /// Prints a formatted help message to the provided output <c>TextWriter</c> with information
    /// about parser usage, arguments, options, and subcommand parsers as formatted by the provided <paramref name="formatter"/>
    /// </summary>
    public void PrintHelp(IParserHelpFormatter<C> formatter, TextWriter writer) => formatter.PrintHelp(this, writer);
    /// <summary>
    /// Returns all flag-like options attached to the parser via the methods <see cref="AddFlag(Flag{C})"/> and <see cref="AddFlags(Flag{C}[])"/>.
    /// </summary>
    public ReadOnlyCollection<Flag<C>> Flags { get; }
    /// <summary>
    /// Attach a subparser to this parser as a subcommand. 
    /// The subparser is then triggered and used to parse the rest of the input after one of the names of the subparser
    /// tokens is found in input.
    /// <para>
    /// See <see cref="Parser{C}"/>.
    /// </para>
    /// </summary>
    /// <param name="commandParser">Parser to attach.</param>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    public IParser<C> AddSubparser(IParser commandParser);
    /// <value>Config context instance. Either passed to the parser in the constructor or created
    /// by the parser right before parsing of input.</value>
    public C? Config { get; }
    /// <summary>
    /// Attach a flag-like option to the parser.
    /// <para>To attach more <see cref="Flag{C}"/> objects at once, see <see cref="AddFlags(Flag{C}[])"/></para>
    /// <para>
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    /// <exception cref="InvalidParserConfigurationException">On invalid parser configuration, e.g. duplicate flag name</exception>
    public IParser<C> AddFlag(Flag<C> flag);
    /// <summary>
    /// Attach one or more flag-like options to the parser.
    /// <para>
    /// See also
    /// </para>
    /// <seealso cref="Flag{C}"/>.
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    /// <exception cref="InvalidParserConfigurationException">On invalid parser configuration, e.g. duplicate flag name</exception>
    public IParser<C> AddFlags(params Flag<C>[] flags);
    /// <summary>
    /// Returns all value options attached to the parser via the methods <see cref="AddFlag(Flag{C})"/> and <see cref="AddFlags(Flag{C}[])"/>.
    /// <para>
    /// See also
    /// <seealso cref="Option{C, V}"/>.
    /// </para>
    /// </summary>
    public ReadOnlyCollection<IOption<C>> Options { get; }
    /// <summary>
    /// Attach a value option to the parser.
    /// <para>To attach more <see cref="IOption{C,V}"/> objects at once, see <see cref="AddOptions(IOption{C}[])"/></para>
    /// <para> See also
    /// <seealso cref="IOption{C, V}"/>
    /// <seealso cref="OptionFactory"/>
    /// </para>
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    /// <exception cref="InvalidParserConfigurationException">On invalid parser configuration, e.g. duplicate option name</exception>
    public IParser<C> AddOption(IOption<C> option);
    /// <summary>
    /// Attach one or more value options to the parser.
    /// <para> See also
    /// <seealso cref="IOption{C, V}"/>,
    /// <seealso cref="OptionFactory"/>
    /// </para>
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    /// <exception cref="InvalidParserConfigurationException">On invalid parser configuration, e.g. duplicate option name</exception>
    public IParser<C> AddOptions(params IOption<C>[] options);
    /// <summary>
    /// Returns all plain arguments attached to the parser via the methods <see cref="AddArgument(IArgument{C}))"/> and <see cref="AddArguments(IArgument{C}[])"/>.
    /// <para>See also <seealso cref="IArgument{C, V}"/>.</para>
    /// </summary>
    public ReadOnlyCollection<IArgument<C>> Arguments { get; }
    /// <summary>
    /// Attach a plain argument to the parser.
    /// <para>To attach more plain arguments at once, use <see cref="AddArguments(IArgument{C}[])"/>.</para>
    /// <para>See also <seealso cref="IArgument{C, V}"/>.</para>
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    /// <exception cref="InvalidParserConfigurationException">If the argument comes
    /// after an argument with multiplicity set to <see cref="ArgumentMultiplicity.AllThatFollow"></exception>
    public IParser<C> AddArgument(IArgument<C> argument);
    /// <summary>
    /// Attach one or more plain arguments to the parser.
    /// <para>See also <seealso cref="IArgument{C, V}"/>.</para>
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    /// <exception cref="InvalidParserConfigurationException">If there is an argument which comes
    /// after an argument with multiplicity set to <see cref="ArgumentMultiplicity.AllThatFollow"></exception>
    public IParser<C> AddArguments(params IArgument<C>[] arguments);
}
