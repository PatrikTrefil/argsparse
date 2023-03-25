using System;

namespace Argparse.Examples;

internal class ComplexExample
{
    record ComplexExampleCommandConfig
    {
        public bool help = false;
    }
    record ComplexExampleSubCommandConfig
    {
        public bool help = false;
        public int? number;
    }

    public static void Run(string[] args)
    {
        var toplevelParser = new Parser<ComplexExampleCommandConfig>(
            () => new ComplexExampleCommandConfig()
            )
        {
            Name = "My program",
            Description = "My description",
            Run = (c,_) => { Console.WriteLine("I will run after my config is ready"); }
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
            Run = (c,_) => { Console.WriteLine("I will run after my config is ready"); }
        };
        subcommandParser.AddFlag(new Flag<ComplexExampleSubCommandConfig>()
        {
            Names = new string[] { "-h", "--help" },
            Description = "Print help",
            Action = (c) => { c.help = true; }
        });

        var numberArg = new Argument<ComplexExampleSubCommandConfig, int>
        {
            ValuePlaceholder = "Number",
            Description = "Number description",
            Action = (storage, value) => { storage.number = value; },
            Converter = ConverterFactory.CreateIntConverter()
        };
        subcommandParser.AddArgument(numberArg);

        toplevelParser.AddSubparser("subcommand", subcommandParser);

        // Now we would run `toplevelParser.ParseAndRun(args);` and
        // one of the provided Run methods would run (or an exception is thrown)
    }
}
