using System;
using System.Collections.Generic;
using System.IO;

namespace Argparse;

/// <summary>
/// Models a <c>Parser</c> of any context.
/// </summary>
public interface IParser
{
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
public record class Parser<C> : IParser
{
    /// <value>The parser name as it will appear in help and debug messages.</value>
    public string Name { get; set; } = "";
    /// <value>The parser description as it will appear in help.</value>
    public string? Description { get; set; }
    /// <value><para><c>PlainArgumentsDelimiter</c> can be used to configure the arguments delimiter,
    /// which when parsed gives signal to the parser to treat all subsequent tokens as plain
    /// arguments.</para>
    /// <para>
    /// Defaults to "--".
    /// </para>
    /// </value>
    public string PlainArgumentsDelimiter { get; set; } = "--";

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
    public Action<C> Run { get; set; } = (_) => { };

    /// <summary>
    /// Returns a dictionary of all atached subparsers with keys being names of the 
    /// commands attached to the parsers.
    /// <para>See <see cref="AddSubparser(string, IParser)"</see></para>
    /// </summary>
    public Dictionary<string, IParser> SubParsers { get; set; } = new();

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
    public Parser<C> AddSubparser(string command, IParser commandParser)
    {
        // validate that command has no spaces and that this command is not already registered
        return this;
    }


    /// <summary>
    /// Returns all flag-like options attached to the parser via the methods <see cref="AddFlag(Flag{C})"/> and <see cref="AddFlags(Flag{C}[])"/>.
    /// </summary>
    public List<Flag<C>> Flags { get; private set; } = new();


    /// <summary>
    /// Attach a flag-like option to the parser.
    /// <para>To attach more <see cref="Flag{C}"/> objects at once, see <see cref="AddFlags(Flag{C}[])"/></para>
    /// <para>
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    public Parser<C> AddFlag(Flag<C> flag)
    {
        // TODO: check that option/flag/command with the same name does not exist
        Flags.Add(flag);
        return this;
    }
    /// <summary>
    /// Attach one or more flag-like options to the parser.
    /// <para>
    /// See also
    /// </para>
    /// <seealso cref="Flag{C}"/>.
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    public Parser<C> AddFlags(params Flag<C>[] flags) { return this; }

    /// <summary>
    /// Returns all value options attached to the parser via the methods <see cref="AddFlag(Flag{C})"/> and <see cref="AddFlags(Flag{C}[])"/>.
    /// <para>
    /// See also
    /// <seealso cref="Option{C, V}"/>.
    /// </para>
    /// </summary>
    public List<IOption<C>> Options { get; private set; } = new();

    /// <summary>
    /// Attach a value option to the parser.
    /// <para>To attach more <see cref="Option{C,V}"/> objects at once, see <see cref="AddOptions(IOption{C}[])"/></para>
    /// <para> See also
    /// <seealso cref="Option{C, V}"/>
    /// <seealso cref="OptionFactory"/>
    /// </para>
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    public Parser<C> AddOption(IOption<C> option)
    {
        // TODO: check that option/flag/command with the same name does not exist
        Options.Add(option);
        return this;
    }
    /// <summary>
    /// Attach one or more value options to the parser.
    /// <para> See also
    /// <seealso cref="Option{C, V}"/>,
    /// <seealso cref="OptionFactory"/>
    /// </para>
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    public Parser<C> AddOptions(params IOption<C>[] options) { return this; }

    /// <summary>
    /// Returns all plain arguments attached to the parser via the methods <see cref="AddArgument(IArgument{C}))"/> and <see cref="AddArguments(IArgument{C}[])"/>.
    /// <para>See also <seealso cref="Argument{C, V}"/>.</para>
    /// </summary>
    public List<IArgument<C>> Arguments { get; private set; } = new();

    /// <summary>
    /// Attach a plain argument to the parser.
    /// <para>To attach more plain arguments at once, use <see cref="AddArguments(IArgument{C}[])"/>.</para>
    /// <para>See also <seealso cref="Argument{C, V}"/>.</para>
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    public Parser<C> AddArgument(IArgument<C> argument)
    {
        // TODO: check that option/flag/command with the same name does not exist
        Arguments.Add(argument);
        return this;

    }
    /// <summary>
    /// Attach one or more plain arguments to the parser.
    /// <para>See also <seealso cref="Argument{C, V}"/>.</para>
    /// </summary>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    public Parser<C> AddArguments(params IArgument<C>[] argument) { return this; }

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
    public void Parse(string[] args) => ParseAndRun(args, true, (_) => { });
    public void ParseAndRun(string[] args) => ParseAndRun(args, isRoot: true, localRun: Run);
    void ParseAndRun(string[] args, bool isRoot, Action<C> localRun)
    {
        throw new NotImplementedException();
        //if (isRoot)
        //{
        //    // determine command and then find parser and run ParseAndRun on
        //    // that parser with the commands removed from the arguments
        //    //ParseAndRun(..., false);
        //}
        //else
        //{
        //    if (Config is null)
        //    {
        //        if (ConfigFactory is null)
        //            throw new Exception("Config and ConfigFactory are both null. This should never happen");
        //        Config = ConfigFactory();
        //    }
        //    // TODO: parse remaining arguments

        //    localRun(Config);
        //}
    }
}
