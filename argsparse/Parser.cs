using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Argsparse;

/// <summary>
/// Models information about a plain argument, how many parts it should consist of.
/// <para>
/// See <see cref="Argument{C, V}"/>, <see cref="IArgument{C}"/>.
/// </para>
/// </summary>
public record class ArgumentMultiplicity
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
    private ArgumentMultiplicity() { }
}

public interface IArgument<C>
{
    /// <summary>
    /// Name of the argument as it should appear in help write-up and debug messages.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Description of the argument as it should appear in help write-up.
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// Value placeholder will be used in synopsis e.g. program [options] <value-placeholder> ...
    /// </summary>
    public string ValuePlaceholder { get; set; }
    /// <summary>
    /// Codifies information about what number or range of numbers of parts for this argument is to be expected.
    /// </summary>
    public ArgumentMultiplicity Multiplicity { get; set; }
    internal void Process(C config, string value);
}

public sealed record class Argument<C, V> : IArgument<C>
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string ValuePlaceholder { get; set; } = "<arg>";
    /// <summary>
    /// Function to convert the argument value parsed as a string from the input to the target type.
    /// </summary>
    public Func<string, V> Converter { get; set; } = (strVal) => default;
    /// <summary>
    /// Action to be carried out upon parsing of the argument in the input.
    /// </summary>
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
    /// Description of the option as it should appear in help write-up.
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// If false, the parser will produce an error state upon finishing parsing without encountering
    /// the option.
    /// </summary>
    public bool IsRequired { get; set; }
    /// <summary>
    /// Value used in help message as placeholder for value.
    /// e.g. when set to FILE for option --output, the help
    /// message will be: --output=FILE
    /// </summary>
    public string ValuePlaceHolder { get; set; }
    internal void Process(C config, string value);
}

/// <summary>
/// Models a parser option which is to accept a value provided by the user.
/// </summary>
/// <typeparam name="C">The cofiguration context type for the given parser, see <see cref="Parser{C}"/></typeparam>
/// <typeparam name="V">Type of the value the provided string is to be converted to. /// </typeparam>
public sealed record class Option<C, V> : IOption<C>
{
    public string[]? Names { get; set; }
    public string ValuePlaceHolder { get; set; } = "<value>";
    public string? Description { get; set; }
    /// <summary>
    /// Action to be carried out upon parsing and conversion of the option from the input.
    /// </summary>
    public Action<C, V> Action { get; set; } = (conf, val) => { };
    public bool IsRequired { get; set; } = false;
    /// <summary>
    /// Function to convert the argument value parsed as a string from the input to the target type.
    /// </summary>
    public Func<string, V> Converter { get; set; } = (strVal) => default;
    void IOption<C>.Process(C config, string value)
    {
        V convertedValue = Converter(value);
        Action(config, convertedValue);
    }
}

/// <summary>
/// A convenience class used for speedy creation of common option types.
/// </summary>
public static class OptionFactory
{
    /// <summary>
    /// Creates a simple string-valued option for a <see cref="Parser{C}"/> 
    /// with config context <typeparamref name="C"/>.
    /// </summary>
    public static Option<C, string> CreateStringOption<C>()
    {
        return new Option<C, string>() { Converter = (string s) => { return s; } };
    }

    /// <summary>
    /// Creates a option which accepts a list of values convertible to type <typeparamref name="T"/>
    /// by the provided convertor function <paramref name="convertor"/>.
    /// </summary>
    /// <typeparam name="C">Config context type of the parent parser.</typeparam>
    /// <typeparam name="T">Type of the individual values listed in the option value.</typeparam>
    /// <param name="convertor">Function to convert the individiual values from string to intended type.</param>
    /// <param name="separator">Separator of the individual values in the option value</param>
    public static Option<C, List<T>> CreateListOption<C, T>(Func<string, T> convertor, char separator = ',')
    {
        return new Option<C, List<T>>() { Converter = (string s) => { return s.Split(separator).Select(x => convertor(x)).ToList(); } };
    }
    /// <summary>
    /// Creates a simple bool-valued option for a <see cref="Parser{C}"/> 
    /// with config context <typeparamref name="C"/>.
    /// </summary>
    public static Option<C, bool> CreateBoolOption<C>()
    {
        return new Option<C, bool>()
        {
            Converter = (string s) =>
            {
                if (s == "true") return true;
                return false;
            }
        };
    }
    /// <summary>
    /// Creates a simple int-valued option for a <see cref="Parser{C}"/> 
    /// with config context <typeparamref name="C"/>.
    /// </summary>
    public static Option<C, int> CreateIntOption<C>()
    {
        return new Option<C, int>()
        {
            Converter = (string s) =>
            {
                return int.Parse(s);
            }
        };
    }
}

/// <summary>
/// A convenience class used for speedy creation of common argument types.
/// </summary>
public static class ArgumentFactory
{
    /// <summary>
    /// Creates a simple string-valued argument for a <see cref="Parser{C}"/> 
    /// with config context <typeparamref name="C"/>.
    /// </summary>
    public static Argument<C, string> CreateStringArgument<C>()
    {
        return new Argument<C, string>() { Converter = (string s) => { return s; } };
    }

    /// <summary>
    /// Creates a argument which accepts a list of values convertible to type <typeparamref name="T"/>
    /// by the provided convertor function <paramref name="convertor"/>.
    /// </summary>
    /// <typeparam name="C">Config context type of the parent parser.</typeparam>
    /// <typeparam name="T">Type of the individual values listed in the argument value.</typeparam>
    /// <param name="convertor">Function to convert the individiual values from string to intended type.</param>
    /// <param name="separator">Separator of the individual values in the argument value</param>
    public static Argument<C, List<T>> CreateListArgument<C, T>(Func<string, T> convertor, char separator = ',')
    {
        return new Argument<C, List<T>>() { Converter = (string s) => { return s.Split(separator).Select(x => convertor(x)).ToList(); } };
    }
    /// <summary>
    /// Creates a simple bool-valued argument for a <see cref="Parser{C}"/> 
    /// with config context <typeparamref name="C"/>.
    /// </summary>
    public static Argument<C, bool> CreateBoolArgument<C>()
    {
        return new Argument<C, bool>()
        {
            Converter = (string s) =>
            {
                if (s == "true") return true;
                return false;
            }
        };
    }
    /// <summary>
    /// Creates a simple int-valued argument for a <see cref="Parser{C}"/> 
    /// with config context <typeparamref name="C"/>.
    /// </summary>
    public static Argument<C, int> CreateIntArgument<C>()
    {
        return new Argument<C, int>()
        {
            Converter = (string s) =>
            {
                return int.Parse(s);
            }
        };
    }
}

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
    public string[]? Names { get; set; }
    /// <summary>
    /// Description of the flag-like option as it should appear in help write-up.
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// Action to be carried out upon parsing of the flag in the input.
    /// </summary>
    public Action<C> Action { get; set; } = (conf) => { };
}

/// <summary>
/// An object which is used to create help write-ups for parsers, the message one usually sees
/// when a command is passed the --help flag.
/// </summary>
/// <typeparam name="T">The cofiguration context type for the given parser, see <see cref="Parser{C}"/></typeparam>
public interface IParserHelpFormatter<T>
{
    /// <summary>
    /// Prints a help write-up to the provided <see cref="TextWriter"/> from the information
    /// gathered from the provided <paramref name="parser"/>.
    /// </summary>
    public abstract void PrintHelp(Parser<T> parser, TextWriter writer);
}

/// <summary>
/// An implementation of <see cref="IParserHelpFormatter{T}"/> that provides basic
/// help formatting which includes all information about options and arguments
/// </summary>
public sealed class DefaultHelpFormatter<T> : IParserHelpFormatter<T>
{

    public void PrintHelp(Parser<T> parser, TextWriter writer)
    {
        System.Console.WriteLine(parser.Name);

        if (parser.Description is not null)
        {
            System.Console.WriteLine();
            System.Console.WriteLine(parser.Description);
        }

        System.Console.WriteLine();

        Console.Write(parser.Name);
        if (parser.Options.Any() || parser.Flags.Any())
            Console.Write(" [options]");
        if (parser.Arguments.Any())
        {
            foreach (var arg in parser.Arguments)
            {
                switch (arg.Multiplicity)
                {
                    case ArgumentMultiplicity.SpecificCount:
                        var argMulSpecCount = arg.Multiplicity as ArgumentMultiplicity.SpecificCount;
                        for (int i = 0; i < argMulSpecCount.Number; i++)
                            Console.Write(" " + arg.ValuePlaceholder);

                        break;
                    case ArgumentMultiplicity.AllThatFollow:
                        Console.Write(" " + arg.ValuePlaceholder + " ...");
                        break;
                }
            }
            Console.WriteLine();
        }

        if (parser.Options.Any())
        {
            Console.WriteLine();
            Console.WriteLine("Options:");
        }

        foreach (var option in parser.Options)
        {
            foreach (var name in option.Names)
            {
                System.Console.Write(name + "=" + option.ValuePlaceHolder);
                System.Console.Write(", ");
            }
            if (option.Description is not null)
            {
                System.Console.Write("- ");
                System.Console.Write(option.Description);
            }
            Console.WriteLine();
        }

        if (parser.Flags.Any())
        {
            Console.WriteLine();
            Console.WriteLine("Flags:");
        }
        foreach (var flag in parser.Flags)
        {
            foreach (var name in flag.Names)
            {
                Console.Write(name);
                Console.Write(", ");
            }
            if (flag.Description is not null)
            {
                Console.Write("- ");
                Console.Write(flag.Description);
            }
            Console.WriteLine();
        }

        if (parser.Arguments.Any())
        {
            Console.WriteLine();
            Console.WriteLine("Arguments:");
        }
        foreach (var arg in parser.Arguments)
        {
            Console.Write(arg.Name);
            if (arg.Description is not null)
            {
                Console.Write("- ");
                Console.Write(arg.Description);
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }


}

/// <summary>
/// Models an expception thrown when the parser is not configured properly, for example with subcommands of the same name.
/// </summary>
public class InvalidParserConfigurationException : Exception
{
    public InvalidParserConfigurationException(string message) : base(message) { }
}

/// <summary>
/// Models an exception thrown when the parser fails to convert a value from string to the intended type.
/// </summary>
public class ParserConversionException : Exception
{

    public ParserConversionException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Models an exception thrown when the parser fails to run the action associated with an option, flag or argument or a parser.
/// </summary>
public class ParserRuntimeException : Exception
{

    public ParserRuntimeException(string message, Exception inner) : base(message, inner) { }
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
    public void ParseAndRun(string[] args) => ParseAndRun(args, true, this.Run);
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
