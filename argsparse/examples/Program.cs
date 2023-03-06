namespace Argsparse.Examples;

class Program
{
    internal class ExampleSubCommandConfiguration
    {
        public bool help = false;
    }

    internal class ExampleConfiguration
    {
        public string? algorithm;
        public bool help = false;
        public ExampleSubCommandConfiguration subCommand = null;
    }

    static void Main(string[] args)
    {
        // If the user does not mind null values in the config, they can just provide
        // the config object directly
        var config = new ExampleConfiguration();



        // If the user want to avoid null values, they can provide a factory method
        // which is called when the parser is first used and use a different
        // config object for each parser. How do they access the results?
        var parser = new Parser<ExampleConfiguration>(config)
            .WithName("program")
            .WithDescription("Program description");

        // Z: would be better as a method, constructors cannot infer type, it seems

        var helpOption = (new Flag<ExampleConfiguration>())
            .WithNames("-h", "--help")
            .WithDescription("Show help")
            .WithAction((storage) => { storage.help = true; });
        parser.addFlag(helpOption);

        var subCommandParser = (new Parser<ExampleConfiguration>(config))
            .WithDescription("Subcommand description")
            .WithName("subcommand")
            .WithAction((storage) => { storage.subCommand = new SubCommandStorage(); });

        // Z: Parser doesn't have action - intended?

        var subCommandHelp = (new Flag<ExampleConfiguration>())
            .WithNames("-h", "--help")
            .WithDescription("Show help")
            .WithAction((storage) => { storage.subCommand.help = true; });
        subCommandParser.addFlag(subCommandHelp);

        var algorithmOption = OptionFactory.CreateStringOption<ExampleConfiguration>()
            .WithNames("-a", "--algorithm")
            .WithDescription("Set algorithm to use")
            .WithAction((storage, value) => { storage.algorithm = value; });

        subCommandParser.addOption(algorithmOption);

        parser.addSubparser(subCommandParser);

        parser.parse(args);

    }
}