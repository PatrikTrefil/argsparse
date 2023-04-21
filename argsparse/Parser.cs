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
    /// <value>The parser name as it will appear in help and debug messages.</value>
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
    void ParseAndRun(string[] args);

    /// <summary>
    /// Parses command line-like input from <paramref name="args"/>.
    /// </summary>
    /// <exception cref="InvalidParserConfigurationException">Thrown when the parser is not configured properly.</exception>
    /// <exception cref="ParserConversionException">Thrown when the parser fails to convert a value from string to the intended type.</exception>
    /// <exception cref="ParserRuntimeException">Thrown when the parser fails to run the action associated with an option, flag or argument or a parser.</exception>
    void Parse(string[] args);
}
public partial record class Parser<C> : IParser
{
    public required string[] Names { get; init; }
    /// <value>The parser description as it will appear in help.</value>
    public required string Description { get; init; }
    /// <value><para><c>PlainArgumentsDelimiter</c> can be used to configure the arguments delimiter,
    /// which when parsed gives signal to the parser to treat all subsequent tokens as plain
    /// arguments.</para>
    /// <para>
    /// Defaults to "--".
    /// </para>
    /// </value>
    public string PlainArgumentsDelimiter { get; init; } = "--";

    /// <value>Config context instance. Either passed to the parser in the constructor or created
    /// by the parser right before parsing of input.</value>
    public C? Config { get; private set; }
    /// <value>
    /// Factory method used to create config context instance when the parser is run.
    /// <para>
    /// <c>Null</c> if the parser is created with a provided config context instance.
    /// </para>
    /// </value>
    public Func<C>? ConfigFactory { get; private set; }

    /// <value>
    /// Delegate to be run after the parser finishes parsing.
    /// <para>
    /// Defaults to an empty action.
    /// </para>
    /// </value>
    public Action<C, Parser<C>> Run { get; set; } = (_, _) => { };

    /// <summary>
    /// Returns a dictionary of all atached subparsers with keys being names of the 
    /// commands attached to the parsers.
    /// <para>See <see cref="AddSubparser(string, IParser)"</see></para>
    /// </summary>
    public ReadOnlyDictionary<string, IParser> SubParsers => this.subparsers.AsReadOnly();

    /// <summary>
    /// Attach a subparser to this parser as a subcommand <paramref name="command"/>. 
    /// The subparser is then triggered and used to parse the rest of the input after the command
    /// token is found in input.
    /// <para>
    /// See <see cref="Parser{C}"/>.
    /// </para>
    /// </summary>
    /// <param name="command">A string command with which the subparser will be triggred. May not contain spaces</param>
    /// <param name="commandParser">Parser to attach.</param>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    public partial Parser<C> AddSubparser(string command, IParser commandParser);


    /// <summary>
    /// Returns all flag-like options attached to the parser via the methods <see cref="AddFlag(Flag{C})"/> and <see cref="AddFlags(Flag{C}[])"/>.
    /// </summary>
    public ReadOnlyCollection<Flag<C>> Flags => flags.AsReadOnly();

    /// <summary>
    /// Attach a flag-like option to the parser.
    /// <para>To attach more <see cref="Flag{C}"/> objects at once, see <see cref="AddFlags(Flag{C}[])"/></para>
    /// <para>
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    public partial Parser<C> AddFlag(Flag<C> flag);
    /// <summary>
    /// Attach one or more flag-like options to the parser.
    /// <para>
    /// See also
    /// </para>
    /// <seealso cref="Flag{C}"/>.
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    public partial Parser<C> AddFlags(params Flag<C>[] flags);
    /// <summary>
    /// Returns all value options attached to the parser via the methods <see cref="AddFlag(Flag{C})"/> and <see cref="AddFlags(Flag{C}[])"/>.
    /// <para>
    /// See also
    /// <seealso cref="Option{C, V}"/>.
    /// </para>
    /// </summary>
    public ReadOnlyCollection<IOption<C>> Options => options.AsReadOnly();

    /// <summary>
    /// Attach a value option to the parser.
    /// <para>To attach more <see cref="Option{C,V}"/> objects at once, see <see cref="AddOptions(IOption{C}[])"/></para>
    /// <para> See also
    /// <seealso cref="Option{C, V}"/>
    /// <seealso cref="OptionFactory"/>
    /// </para>
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    public partial Parser<C> AddOption(IOption<C> option);
    /// <summary>
    /// Attach one or more value options to the parser.
    /// <para> See also
    /// <seealso cref="Option{C, V}"/>,
    /// <seealso cref="OptionFactory"/>
    /// </para>
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    public partial Parser<C> AddOptions(params IOption<C>[] options);

    /// <summary>
    /// Returns all plain arguments attached to the parser via the methods <see cref="AddArgument(IArgument{C}))"/> and <see cref="AddArguments(IArgument{C}[])"/>.
    /// <para>See also <seealso cref="Argument{C, V}"/>.</para>
    /// </summary>
    public ReadOnlyCollection<IArgument<C>> Arguments => plainArguments.AsReadOnly();

    /// <summary>
    /// Attach a plain argument to the parser.
    /// <para>To attach more plain arguments at once, use <see cref="AddArguments(IArgument{C}[])"/>.</para>
    /// <para>See also <seealso cref="Argument{C, V}"/>.</para>
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    /// <exception cref="InvalidParserConfigurationException">If the argument comes
    /// after an argument with multiplicity set to <see cref="ArgumentMultiplicity.AllThatFollow"></exception>
    public partial Parser<C> AddArgument(IArgument<C> argument);

    /// <summary>
    /// Attach one or more plain arguments to the parser.
    /// <para>See also <seealso cref="Argument{C, V}"/>.</para>
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    /// <exception cref="InvalidParserConfigurationException">If there is an argument which comes
    /// after an argument with multiplicity set to <see cref="ArgumentMultiplicity.AllThatFollow"></exception>
    public partial Parser<C> AddArguments(params IArgument<C>[] arguments);

    /// <summary>
    /// Creates parser with config context <typeparamref name="C"/>.
    /// </summary>
    /// <param name="config">Instance of config context to be used.</param>
    public Parser(C config)
    {
        Config = config;
    }

    /// <summary>
    /// Creates parser with config context <typeparamref name="C"/>.
    /// </summary>
    /// <param name="configFactory">Factory method to be used to instantiate the config object when
    /// the parser is used.</param>
    public Parser(Func<C> configFactory)
    {
        ConfigFactory = configFactory;
    }

    /// <summary>
    /// Prints a formatted help message to the provided output <c>TextWriter</c> with information
    /// about parser usage, arguments, options, and subcommand parsers as formatted by the provided <paramref name="formatter"/>
    /// </summary>
    public void PrintHelp(IParserHelpFormatter<C> formatter, TextWriter writer) => formatter.PrintHelp(this, writer);
    /// <summary>
    /// Prints a formatted help message to the provided output <c>TextWriter</c> with information
    /// about parser usage, arguments, options, and subcommand parsers as formatted by <see cref="DefaultHelpFormatter{T}"/>
    /// </summary>
    public void PrintHelp(TextWriter writer) => PrintHelp(new DefaultHelpFormatter<C>(), writer);
    /// <summary>
    /// Prints a formatted help message to the console with information
    /// about parser usage, arguments, options, and subcommand parsers as formatted by the provided <paramref name="formatter"/>.
    /// </summary>
    public void PrintHelp(IParserHelpFormatter<C> formatter) => PrintHelp(formatter, System.Console.Out);
    /// <summary>
    /// Prints a formatted help message to the console with information
    /// about parser usage, arguments, options, and subcommand parsers as formatted by <see cref="DefaultHelpFormatter{T}"/>
    /// </summary>
    public void PrintHelp() => PrintHelp(new DefaultHelpFormatter<C>(), System.Console.Out);
    public void Parse(string[] args) => ParseAndRun(args, true, (_, _) => { });
    public void ParseAndRun(string[] args) => ParseAndRun(args, isRoot: true, localRun: Run);
    partial void ParseAndRun(string[] args, bool isRoot, Action<C, Parser<C>> localRun);
}
