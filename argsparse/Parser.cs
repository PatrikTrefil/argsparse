using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Argsparse;

// TODO: maybe suppor subarsers again? by providing Run delegate

/*
Documentation

Option a Flag jsou dvě různé úplně nezávislé třídy.
Option existuje pouze jeden a nemůže se od něj dědit.
Pokud chceme vyrobit novou Option s předdefinovaným chováním (např. konverzí)
tak použijeme OptionFactory (vyhneme se tím dedičnosti, což věci zjednodušuje).

Všechny třídy jsou mutable a podporují fluent syntax pomocí klíčového slova
with. Všechny třídy jsou vždy validní instance.

Fluent syntax je pouze jedna možnost jak třídy používat. Je možno
vše vyplňovat ručně např. pomocí object initializeru. Zároveň to umožňuje uživateli číst konfiguraci.

Obj. initializer příklad:

```
var flag = new Flag<Config>
{
    Names = new[] { "-h", "--help" },
    Description = "Print help",
    Action = (config, value) => { config.PrintHelp = true; }
};
```

Tisk helpu je možné zavolat přímo na parseru, ale logika tisku je
v samostatné třídě. Poskytujeme defaultní implementaci, ale
uživatel si může napsat vlastní tím že implementuje interface ParserHelpFormatter

Jednotlivé parsery mohou ukládat data do instance na kterou dostane ukazatel
nebo dostane factory metodu a instanci si vyrobí sám pokud ji bude potřebovat
(umožňuje se vyhnout spoustě null hodnotám - viz příklad níže)

Používám recordy, protože nám zadarmo dává print metodu, copy ctor, hashing, eq comparison.
*/

public record ArgumentMultiplicity
{
    public record SpecificCount(int Number, bool IsRequired) : ArgumentMultiplicity();
    public record AllThatFollow(int MinimumNumberOfArguments = 0) : ArgumentMultiplicity();
    private ArgumentMultiplicity() { }
}

public interface IArgument<C>
{
    public string Name { get; set; }
    public string Description { get; set; }
    /// <summary>
    /// Value placeholder will be used in synopsis e.g. program [options] <value-placeholder> ...
    /// </summary>
    public string ValuePlaceholder { get; set; }
    public ArgumentMultiplicity Multiplicity { get; set; }
    internal void Process(C config, string value);
}

public sealed record Argument<C, V> : IArgument<C>
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ValuePlaceholder { get; set; } = "<arg>";
    public Func<string, V> Converter { get; set; } = (strVal) => default;
    public Action<C, V> Action { get; set; } = (conf, val) => { };
    public ArgumentMultiplicity Multiplicity { get; set; } = new ArgumentMultiplicity.SpecificCount(Number: 1, IsRequired: true);
    void IArgument<C>.Process(C config, string value)
    {
        V convertedValue = Converter(value);
        Action(config, convertedValue);
    }
}

public interface IOption<C>
{
    /// <summary>
    /// Names the options as they will be parsed by the parser.
    /// Names of long options are prefixed with two dashes '--'
    /// Names of short options are prefixed with one dash '-'
    /// One option can represent both long and short options
    /// </summary>
    public string[]? Names { get; set; }
    
    /// <summary>
    /// Description will be used in help message.
    /// </summary>
    public string Description { get; set; }
    public bool IsRequired { get; set; }
    /// <summary>
    /// Value used in help message as placeholder for value.
    /// e.g. when set to FILE for option --output, the help
    /// message will be: --output=FILE
    /// </summary>
    public string ValuePlaceHolder { get; set; }
    internal void Process(C config, string value);
}

public sealed record Option<C, V> : IOption<C>
{
    public string[]? Names { get; set; }
    public string ValuePlaceHolder { get; set; } = "<value>";
    public string Description { get; set; } = "";
    public Action<C, V> Action { get; set; } = (conf, val) => { };
    public bool IsRequired { get; set; } = false;
    public Func<string, V> Converter { get; set; } = (strVal) => default;
    void IOption<C>.Process(C config, string value)
    {
        V convertedValue = Converter(value);
        Action(config, convertedValue);
    }
}

public static class OptionFactory
{
    public static Option<C, string> CreateStringOption<C>()
    {
        return new Option<C, string>() { Converter = (string s) => { return s; } };
    }
    public static Option<C, List<T>> CreateListOption<C, T>(Func<string, T> convertor, char separator = ',')
    {
        return new Option<C, List<T>>() { Converter = (string s) => { return s.Split(separator).Select(x => convertor(x)).ToList(); } };
    }
}

public sealed record Flag<C>
{
    public string[]? Names { get; set; }
    public string Description { get; set; } = "";
    public Action<C> Action { get; set; } = (conf) => { };
}

public interface IParserHelpFormatter<T>
{
    public abstract void PrintHelp(Parser<T> parser, TextWriter writer);
}

public sealed class DefaultHelpFormatter<T> : IParserHelpFormatter<T>
{
    public void PrintHelp(Parser<T> parser, TextWriter writer)
    {
        System.Console.WriteLine(parser.Name);
        System.Console.WriteLine();
        System.Console.WriteLine(parser.Description);
        System.Console.WriteLine();

        Console.Write(parser.Name);
        if (parser.GetOptions().Any() || parser.GetFlags().Any())
            Console.Write(" [options]");
        foreach (var arg in parser.GetArguments())
        {
            Console.Write(" " + arg.ValuePlaceholder);
            switch (arg.Multiplicity)
            {
                case ArgumentMultiplicity.SpecificCount:
                    var argMulSpecCount = arg.Multiplicity as ArgumentMultiplicity.SpecificCount;
                    for (int i = 0; i < argMulSpecCount.Number; i++)
                        Console.Write(" " + arg.ValuePlaceholder);

                    break;
                case ArgumentMultiplicity.AllThatFollow:
                    Console.Write(arg.ValuePlaceholder + " ...");
                    break;
            }
            Console.WriteLine();
        }

        if (parser.GetOptions().Any())
        {
            Console.WriteLine();
            Console.WriteLine("Options:");
        }

        foreach (var option in parser.GetOptions())
        {
            foreach (var name in option.Names)
            {
                System.Console.Write(name + "=" + option.ValuePlaceHolder);
                System.Console.Write(", ");
            }
            System.Console.Write("- ");
            System.Console.WriteLine(option.Description);
        }

        if (parser.GetFlags().Any())
        {
            Console.WriteLine();
            Console.WriteLine("Flags:");
        }
        foreach (var flag in parser.GetFlags())
        {
            foreach (var name in flag.Names)
            {
                Console.Write(name);
                Console.Write(", ");
            }
            Console.Write("- ");
            Console.WriteLine(flag.Description);
        }

        if (parser.GetArguments().Any())
        {
            Console.WriteLine();
            Console.WriteLine("Arguments:");
        }
        foreach (var arg in parser.GetArguments())
        {
            Console.Write(arg.Name);
            Console.Write("- ");
            Console.WriteLine(arg.Description);
        }
        Console.WriteLine();
    }


}

/// <summary>
/// Models a <c>Parser</c> of any context.
/// </summary>
public interface IParser
{
    /// <summary>
    /// Parses command line-like input from <paramref name="args"/> and then invoke
    /// the action provided to the specific parser in <c>Run</c>.
    /// </summary>
    void ParseAndRun(string[] args);

    /// <summary>
    /// Parses command line-like input from <paramref name="args"/>.
    /// </summary>
    void Parse(string[] args);
}
public record Parser<C> : IParser
{
    /// <value>The parser name as it will appear in help and debug messages.</value>
    public string Name { get; set; } = "";
    /// <value>The parser description as it will appear in help.</value>
    public string Description { get; set; } = "";
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
    Dictionary<string, IParser> SubParsers { get; set; } = new();

    /// <summary>
    /// Returns a dictionary of all atached subparsers with keys being names of the 
    /// commands attached to the parsers.
    /// </summary>
    public IReadOnlyDictionary<string, IParser> GetSubparsers() => SubParsers;
    /// <summary>
    /// Attach a subparser to this parser as a subcommand <paramref name="command"/>. 
    /// The subparser is then triggered and used to parse the rest of the input after the command
    /// token is found in input.
    /// </summary>
    /// <param name="command">A string command with which the subparser will be triggred. May not contain spaces</param>
    /// <param name="commandParser">Parser to attach.</param>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    public Parser<C> AddSubparser(string command, IParser commandParser)
    {
        // validate that command has no spaces and that this command is not already registered
        return this;
    }


    protected List<Flag<C>> Flags { get; set; } = new();
    /// <summary>
    /// Returns all flag-like options attached to the parser via the methods <see cref="AddFlag(Flag{C})"/> and <see cref="AddFlags(Flag{C}[])"/>.
    /// </summary>
    public IReadOnlyList<Flag<C>> GetFlags() => Flags;

    /// <summary>
    /// Attach a flag-like option to the parser.
    /// <para>To attach more <see cref="Flag{C}"/> objects at once, see <see cref="AddFlags(Flag{C}[])"/></para>
    /// </summary>
    /// <seealso cref="Flag{C}"/>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    public Parser<C> AddFlag(Flag<C> flag)
    {
        // TODO: check that option/flag/command with the same name does not exist
        Flags.Add(flag);
        return this;
    }
    /// <summary>
    /// Attach one or more flag-like options to the parser.
    /// 
    /// </summary>
    /// <seealso cref="Flag{C}"/>
    /// <returns>The parent parser as to allow for chaining of calls and fluent syntax.</returns>
    public Parser<C> AddFlags(params Flag<C>[] flags) { return this; }

    protected List<IOption<C>> Options { get; set; } = new();
    /// <summary>
    /// Returns all value options attached to the parser via the methods <see cref="AddFlag(Flag{C})"/> and <see cref="AddFlags(Flag{C}[])"/>.
    /// <para>
    /// See also
    /// <seealso cref="Option{C, V}"/>.
    /// </para>
    /// </summary>
    public IReadOnlyList<IOption<C>> GetOptions() => Options;
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
    protected List<IArgument<C>> Arguments { get; private set; } = new();
    /// <summary>
    /// Returns all plain arguments attached to the parser via the methods <see cref="AddArgument(IArgument{C}))"/> and <see cref="AddArguments(IArgument{C}[])"/>.
    /// <para>See also <seealso cref="Argument{C, V}"/>.</para>
    /// </summary>
    public IReadOnlyList<IArgument<C>> GetArguments() => Arguments;

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

    public Parser(C config)
    {
        Config = config;
    }
    public Parser(Func<C> configFactory)
    {
        ConfigFactory = configFactory;
    }

    public void PrintHelp(IParserHelpFormatter<C> formatter, TextWriter writer) => formatter.PrintHelp(this, writer);
    public void PrintHelp(TextWriter writer) => PrintHelp(new DefaultHelpFormatter<C>(), writer);
    public void PrintHelp(IParserHelpFormatter<C> formatter) => PrintHelp(formatter, System.Console.Out);
    public void PrintHelp() => PrintHelp(new DefaultHelpFormatter<C>(), System.Console.Out);

    public void ParseAndRun(string[] args) => ParseAndRun(args, true, Run);
    public void Parse(string[] args) => ParseAndRun(args, true, (_) => { });
    void ParseAndRun(string[] args, bool isRoot, Action<C> localRun)
    {
        throw new NotImplementedException();
        if (isRoot)
        {
            // determine command and then find parser and run ParseAndRun on
            // that parser with the commands removed from the arguments
            //ParseAndRun(..., false);
        }
        else
        {
            if (Config is null)
            {
                if (ConfigFactory is null)
                    throw new Exception("Config and ConfigFactory are both null. This should never happen");
                Config = ConfigFactory();
            }
            // TODO: parse remaining arguments

            localRun(Config);
        }
    }
}
