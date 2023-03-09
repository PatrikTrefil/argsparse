using System;

namespace Argsparse.Examples;

class Program
{

    internal record ExampleConfiguration
    {
        public string? algorithm;
        public bool help = false;
    }

    static void SimpleExample(string[] args)
    {
        var config = new ExampleConfiguration();

        var parser = new Parser<ExampleConfiguration>(config)
        {
            Name = "program",
            Description = "Program description"
        };

        var helpFlag = new Flag<ExampleConfiguration>()
        {
            Names = new string[] { "-h", "--help" },
            Description = "Show help",
            Action = (storage) => { storage.help = true; }
        };

        parser.AddFlag(helpFlag);

        var algorithmOption = OptionFactory.CreateStringOption<ExampleConfiguration>() with
        {
            Names = new string[] { "-a", "--algorithm" },
            Description = "Set algorithm to use",
            Action = (storage, value) => { storage.algorithm = value; }
        };

        parser.AddOption(algorithmOption);

        parser.Parse(args);
    }
    internal record ComplexExampleCommandConfig
    {
        public bool help = false;
    }
    internal record ComplexExampleSubCommandConfig
    {
        public bool help = false;
    }

    static void ComplexExample(string[] args)
    {
        var toplevelParser = new Parser<ComplexExampleCommandConfig>(
            () => new ComplexExampleCommandConfig()
            )
        {
            Name = "My program",
            Description = "My description",
            Run = (c) => { Console.WriteLine("I will run after my config is ready"); }
        };
        toplevelParser.AddFlag(new Flag<ComplexExampleCommandConfig>
        {
            Names = new string[] { "-h", "--help" },
            Description = "Print help",
            Action = (c) => { c.help = true; }
        });
        var subcommandParser = new Parser<ComplexExampleSubCommandConfig>(
            () => new ComplexExampleSubCommandConfig()
            )
        {
            Name = "My subcommand",
            Description = "My subcommand description",
            Run = (c) => { Console.WriteLine("I will run after my config is ready"); }
        };
        subcommandParser.AddFlag(new Flag<ComplexExampleSubCommandConfig>()
        {
            Names = new string[] { "-h", "--help" },
            Description = "Print help"
        });
        toplevelParser.AddSubparser("subcommand", subcommandParser);
        toplevelParser.ParseAndRun(args);
    }

    static void Main(string[] args)
    {
        SimpleExample(args);
        ComplexExample(args);
    }
}