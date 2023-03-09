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
    public string ValuePlaceholder { get; set; } = "";
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
    public string[]? Names { get; set; }
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
    public string ValuePlaceHolder { get; set; } = "";
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
        foreach (var option in parser.GetOptions())
        {
            foreach (var name in option.Names)
            {
                System.Console.Write(name + "=" + option.ValuePlaceHolder);
                System.Console.Write(" ");
            }
            System.Console.WriteLine("- ");
            System.Console.WriteLine(option.Description);
        }
        foreach (var flag in parser.GetFlags())
        {
            foreach (var name in flag.Names)
            {
                Console.Write(name);
                Console.Write(" ");
            }
            Console.WriteLine();
            Console.WriteLine(flag.Description);
        }
        foreach (var arg in parser.GetArguments())
        {
            foreach (var name in arg.Names)
            {
                Console.WriteLine(name);
                Console.WriteLine(" ");
            }
            Console.WriteLine("- ");
            Console.WriteLine(arg.Description);
        }
    }


}


public record Parser<C>
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string PlainArgumentsDelimiter { get; set; } = "--";
    public C Config { get; set; }
    public Func<C>? ConfigFactory { get; set; } = null;

    protected List<Flag<C>> Flags { get; set; } = new();
    public IReadOnlyList<Flag<C>> GetFlags() => Flags;
    public Parser<C> AddFlag(Flag<C> flag) { return this; }
    public Parser<C> AddFlags(params Flag<C>[] flags) { return this; }

    protected List<IOption<C>> Options { get; set; } = new();
    public IReadOnlyList<IOption<C>> GetOptions() => Options;
    public Parser<C> AddOption(IOption<C> option) { return this; }
    public Parser<C> AddOptions(params IOption<C>[] options) { return this; }


    protected List<IArgument<C>> Arguments { get; private set; } = new();
    public IReadOnlyList<IArgument<C>> GetArguments() => Arguments;
    public Parser<C> AddArgument(IArgument<C> argument) { return this; }
    public Parser<C> AddArguments(params IArgument<C>[] argument) { return this; }

    public Parser(C config)
    {
        Config = config;
    }

    public void PrintHelp(IParserHelpFormatter<C> formatter, TextWriter writer) { throw new NotImplementedException(); }
    public void PrintHelp(TextWriter writer) => PrintHelp(new DefaultHelpFormatter<C>(), writer);
    public void PrintHelp(IParserHelpFormatter<C> formatter) => PrintHelp(formatter, System.Console.Out);
    public void PrintHelp() => PrintHelp(new DefaultHelpFormatter<C>(), System.Console.Out);

    public void Parse(string[] args) { throw new NotImplementedException(); }
}
