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
sealed record Option<C, V>
{
    public string[] Names { get; set; } = "";
    public string Description { get; set; } = "";
    public  Action<C, V> Action { get; set; } = (conf, val) => { };
    public V? DefaultValue { get; set; } = null;
    public bool IsRequired { get; set; } = false;
    public Option<C, V> WithConverter(Func<string, V> converter) { }
    public Option<C, V> WithAction(Action<C, V> action) { }
    public Option<C, V> WithNames(params string[] names) {  }
    public Option<C, V> WithDescription(string description) { }
    public Option<C, V> WithDefaultValue(V defaultValue) { }
    public Option<C, V> Required() {}
}

static sealed class OptionFactory
{
    public static Option<string> CreateStringOption()
    {
        return new Option<string>().WithConverter((string s) => { return s; });
    }
    public static Option<List<T>> CreateListOption<T>(char separator = ',') {
        return new Option<List<T>>()
            .WithConverter((string s) => { return s.Split(separator).ToList(); });
    }
}

sealed record Flag<C>
{
    string[] Names { get; set; } = "";
    string Description { get; set; } = "";
    Action<C> Action { get; set; } = (conf) => { };
    public WithAction(Action<C> action) { }
    public Flag<C> WithNames(params string[] names) {  }
    public Flag<C> WithDescription(string description) { }
}

interface ParserHelpFormatter<T> {
    abstract void PrintHelp(Parser<T> parser, StreamWriter writer);
}

sealed class DefaultHelpFormatter<T>: ParserHelpFormatter<T> {
    void PrintHelp(Parser<T> parser, StreamWriter writer) {
        System.Console.WriteLine(parser.Name);
        System.Console.WriteLine();
        System.Console.WriteLine(parser.Description);
        System.Console.WriteLine();
        foreach (var option in parser.Options) {
            foreach (var name in option.Names) {
                System.Console.Write(name);
                System.Console.Write(" ");
            }
            System.Console.WriteLine("- ");
            System.Console.WriteLine(option.Description);
        }
    }
}

record Parser<T>
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string PlainArgumentsDelimiter { get; set; } = "--";
    public T Config { get; set; }
    public Func<T> ConfigFactory { get; set; } = null;
    public List<Flag> Flags { get; }
    public List<Option> Options { get; }
    public HashMap<string, Parser> CommandSubparserMap { get; }

    public Parser(T config) { }
    public Parser(Func<T> configFactory) { }
    
    public Parser<T> addOption() { }
    public Parser<T> addFlag() { }
    public Parser<T> addSubparser() { }
    public Parser<T> WithName(string name) { }
    public Parser<T> WithDescription(string description) { }
    public Parser<T> WithPlainArgumentsDelimiter(string delimiter) {}

    public void PrintHelp(ParserHelpFormatter formatter = DefaultHelpFormatter, StreamWriter writer = System.Console.Out) {}
    
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

// #region ExampleUsage

class ExampleSubCommandConfiguration
{
    bool help = false;
}

class ExampleConfiguration
{
    bool help = false;
    ExampleSubCommandConfiguration subCommand = null;
}

class Program
{
    static void Main(string[] args)
    {
        // If the user does not mind null values in the config, they can just provide
        // the config object directly
        ExampleConfiguration config = new ExampleConfiguration();
        // If the user want to avoid null values, they can provide a factory method
        // which is called when the parser is first used and use a different
        // config object for each parser. How do they access the results?
        Parser parser = new Parser(config).WithName("program").WithDescription("Program description");

        Flag helpOption = (new Flag()).WithNames("-h", "--help").WithDescription("Show help").WithAction((storage) => { storage.help = true; });
        parser.addFlag(helpOption);

        Parser subCommandParser = (new Parser(config)).WithDescription("Subcommand description").WithName("subcommand").WithAction((storage) => { storage.subCommand = new SubCommandStorage(); });

        Flag subCommandHelp = (new Flag()).WithNames("-h", "--help").WithDescription("Show help").WithAction((storage) => { storage.subCommand.help = true; });
        subCommandParser.addFlag(subCommandHelp);

        Option algorithmOption = OptionFactory.CreateStringOption().WithNames("-a", "--algorithm").WithDescription("Set algorithm to use").WithAction((storage, value) => { storage.algorithm = value; });
        subCommandParser.addOption(algorithmOption);

        parser.addSubparser(subCommandParser);

        parser.parse(args);

    }
}
// #endregion