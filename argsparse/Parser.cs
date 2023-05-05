using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Argparse;

public partial record class Parser<C> : IParser<C>
{
    /// <value>
    /// Backing field of <see cref="Names"/>
    /// </value>
    private HashSet<string> names;
    /// <inheritdoc/>
    public required HashSet<string> Names
    {
        get => names; init
        {
            if (value.Count == 0)
                throw new ArgumentException("Parser must have at least one name");

            var invalidOptionNames = value.Where(name => name[0] == '-' || name[..2] == "--");
            if (invalidOptionNames.Any())
                throw new ArgumentException($"Invalid parser names: {string.Join(", ", invalidOptionNames)}");

            names = value;
        }
    }
    /// <inheritdoc/>
    public required string Description { get; init; }
    /// <inheritdoc/>
    public string PlainArgumentsDelimiter { get; init; } = "--";
    /// <inheritdoc/>
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
    /// <inheritdoc/>
    public ReadOnlyDictionary<string, IParser> SubParsers => subparsers.AsReadOnly();
    /// <inheritdoc/>
    public partial IParser<C> AddSubparser(IParser commandParser);
    /// <inheritdoc/>
    public ReadOnlyCollection<Flag<C>> Flags => flags.AsReadOnly();
    /// <inheritdoc/>
    public partial IParser<C> AddFlag(Flag<C> flag);
    /// <inheritdoc/>
    public partial IParser<C> AddFlags(params Flag<C>[] flags);
    /// <inheritdoc/>
    public ReadOnlyCollection<IOption<C>> Options => options.AsReadOnly();
    /// <inheritdoc/>
    public partial IParser<C> AddOption(IOption<C> option);
    /// <inheritdoc/>
    public partial IParser<C> AddOptions(params IOption<C>[] options);
    /// <inheritdoc/>
    public ReadOnlyCollection<IArgument<C>> Arguments => plainArguments.AsReadOnly();
    /// <inheritdoc/>
    public partial IParser<C> AddArgument(IArgument<C> argument);
    /// <inheritdoc/>
    public partial IParser<C> AddArguments(params IArgument<C>[] arguments);

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

    /// <inheritdoc/>
    public void PrintHelp(IParserHelpFormatter<C> formatter, TextWriter writer) => formatter.PrintHelp(this, writer);
    /// <summary>
    /// Prints a formatted help message to the provided output <c>TextWriter</c> with information
    /// about parser usage, arguments, options, and subcommand parsers as formatted by <see cref="DefaultHelpFormatter{T}"/>
    /// </summary>
    public void PrintHelp(TextWriter writer) => PrintHelp(new DefaultHelpFormatter<C>(), writer);
    /// <inheritdoc/>
    public void PrintHelp(IParserHelpFormatter<C> formatter) => PrintHelp(formatter, Console.Out);
    /// <summary>
    /// Prints a formatted help message to the console with information
    /// about parser usage, arguments, options, and subcommand parsers as formatted by <see cref="DefaultHelpFormatter{T}"/>
    /// </summary>
    public void PrintHelp() => PrintHelp(new DefaultHelpFormatter<C>(), Console.Out);
    /// <inheritdoc/>
    public void Parse(IEnumerable<string> args)
    {
        ProcessCommands(args, out IParser selectedParser, out IEnumerable<string> argsWithoutCommands);
        selectedParser.ParseWithoutCommands(argsWithoutCommands);
    }
    /// <inheritdoc/>
    public void ParseAndRun(IEnumerable<string> args)
    {
        ProcessCommands(args, out IParser selectedParser, out IEnumerable<string> argsWithoutCommands);
        selectedParser.ParseWithoutCommandsAndRun(argsWithoutCommands);
    }
    /// <inheritdoc/>
    void IParser.ParseWithoutCommandsAndRun(IEnumerable<string> args)
    {
        if (Config is null)
        {
            if (ConfigFactory is null)
                throw new InvalidParserConfigurationException("Config and ConfigFactory are both null. This should never happen");
            Config = ConfigFactory();
        }

        (this as IParser).ParseWithoutCommands(args);
        Run(Config, this);
    }
}
