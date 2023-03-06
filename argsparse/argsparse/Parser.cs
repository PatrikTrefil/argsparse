using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace Argsparse;

// TODO: plain arguments configuration
// TODO: class vs struct for Flag, Option, Parser

/*
Documentation

Option a Flag jsou dvě různé úplně nezávislé třídy.
Option existuje pouze jeden a nemůže se od něj dědit.
Pokud chceme vyrobit novou Option s předdefinovaným chováním (např. konverzí)
tak použijeme OptionFactory (vyhneme se tím dedičnosti, což věci zjednodušuje).

Všechny třídy jsou mutable a podporují fluent syntax. Všechny třídy
jsou vždy validní instance (i když nezavoláme WithAction, tak program nespadne
ale výchozí chování je, že obsahuje prázdnou akci).

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

// #region ArgLib

public interface Option<C>
{
    public string[]? Names { get; set; }
    public string Description { get; set; }
}

public sealed record Option<C, V> : Option<C>
{
    public string[]? Names { get; set; }
    public string Description { get; set; } = "";
    public Action<C, V> Action { get; set; } = (conf, val) => { };
    public V? DefaultValue { get; set; } = default;
    public bool IsRequired { get; set; } = false;
    public Option<C, V> WithConverter(Func<string, V> converter) => default;
    public Option<C, V> WithAction(Action<C, V> action) => default;
    public Option<C, V> WithNames(params string[] names) => default;
    public Option<C, V> WithDescription(string description) => default;
    public Option<C, V> WithDefaultValue(V defaultValue) => default;
    public Option<C, V> Required() => default;
}

public static class OptionFactory
{
    public static Option<C, string> CreateStringOption<C>()
    {
        return new Option<C, string>().WithConverter((string s) => { return s; });
    }
    public static Option<C, List<T>> CreateListOption<C, T>( Func<string, T> convertor, char separator = ',')
    {
        return new Option<C, List<T>>()
            .WithConverter((string s) => { return s.Split(separator).Select(x => convertor(x)).ToList(); });
    }
}

public sealed record Flag<C>
{
    string[]? Names { get; set; };
    string Description { get; set; } = "";
    Action<C> Action { get; set; } = (conf) => { };
    public Flag<C> WithAction(Action<C> action) => default;
    public Flag<C> WithNames(params string[] names) => default;
    public Flag<C> WithDescription(string description) => default;
}

public interface ParserHelpFormatter<T>
{
    public abstract void PrintHelp(Parser<T> parser, StreamWriter writer);
}

public sealed class DefaultHelpFormatter<T> : ParserHelpFormatter<T>
{
    public void PrintHelp(Parser<T> parser, StreamWriter writer)
    {
        System.Console.WriteLine(parser.Name);
        System.Console.WriteLine();
        System.Console.WriteLine(parser.Description);
        System.Console.WriteLine();
        foreach (var option in parser.Options)
        {
            foreach (var name in option.Names)
            {
                System.Console.Write(name);
                System.Console.Write(" ");
            }
            System.Console.WriteLine("- ");
            System.Console.WriteLine(option.Description);
        }
    }

  
}

public record Parser<T>
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string PlainArgumentsDelimiter { get; set; } = "--";
    public T Config { get; set; }
    public Func<T> ConfigFactory { get; set; } = null;
    public List<Flag<T>> Flags { get; }
    public List<Option<T>> Options { get; }
    public Dictionary<string, Parser<T>> CommandSubparserMap { get; }

    public Parser(T config) { }
    public Parser(Func<T> configFactory) { }

    public Parser<T> addOption(Option<T> option) { return default; }
    public Parser<T> addFlag(Flag<T> flag) { return default; }
    public Parser<T> addSubparser() { return default; }
    public Parser<T> WithName(string name) { return default; }
    public Parser<T> WithDescription(string description) { return default; }
    public Parser<T> WithPlainArgumentsDelimiter(string delimiter) { return default; }

    public void PrintHelp(ParserHelpFormatter<T> formatter, StreamWriter writer) { }

    public void parse(string[] args)
    {
        // run factory method on config if it's not null
        foreach (var arg in args)
        {
            // parse flags and options
            // if we find arg that is not a flag or option, we check if it is a subcommand
            // if it's a subcommand, we run the delegate associated with this subcommand and let the subparser parse
            // the rest of the args
            // if it's not a subcommand, we add it to args list 
        }
    }
}

// #endregion
