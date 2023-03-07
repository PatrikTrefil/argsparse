using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace Argsparse;

// TODO: plain arguments configuration
// TODO: readme
// TODO: docstrings
// TODO: example time program
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

public sealed record Argument<C, V>
{
    public static Argument<C, V> Create() => default;

    public string[]? Names { get; set; }
    public string Description { get; set; } = "";
    public Action<C, V> Action { get; set; } = (conf, val) => { };
    public V? DefaultValue { get; set; } = default;
    public bool IsRequired { get; set; } = false;
    // TODO: fluent syntax
    // TODO: multiplicita
}

public interface Option<C>
{
    public string[]? Names { get; set; }
    public string Description { get; set; }
    
    internal void Process(C config, string value);
}

public sealed record Option<C, V> : Option<C>
{
    public static Option<C, V> Create() => default;
    
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

    internal void Process(C config, string value)
    {
        throw new NotImplementedException();
    }

    void Option<C>.Process(C config, string value)
    {
        throw new NotImplementedException();
    }
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
    public static Flag<C> Create ()
    {
        return default;    
    }

    private Flag()
    {
    }

    string[]? Names { get; set; }
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
    

    public static Parser<T> Create(T config)
    {
        return default;
    }

    protected Parser(T config) { }
    
    public Parser<T> AddOption(Option<T> option) { return default; }
    public Parser<T> WithOptions(List<Option<T>> option) { return default; }
    public Parser<T> AddFlag(Flag<T> flag) { return default; }
    public Parser<T> WithFlags(List<Flag<T>> flags) { return default; }
    public Parser<T> WithName(string name) { return default; }
    public Parser<T> WithDescription(string description) { return default; }
    public Parser<T> WithPlainArgumentsDelimiter(string delimiter) { return default; }
    public Parser<T> WithAction(Action<T> action) => default;

    
    public void PrintHelp(ParserHelpFormatter<T> formatter , StreamWriter writer) { }
    public void PrintHelp(StreamWriter writer) { }
    public void PrintHelp(ParserHelpFormatter<T> formatter) { }
    public void PrintHelp() { }


    public void Parse(string[] args)
    {
        foreach (var arg in args)
        {

        }
    }
}

// #endregion
