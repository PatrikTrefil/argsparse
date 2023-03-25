namespace Argparse.Examples;
internal class SimpleExample
{
    record ExampleConfiguration
    {
        public string? algorithm;
        public bool help = false;
        public string? inputFile;
    }

    public static void Run(string[] args)
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

        var algorithmOption = new Option<ExampleConfiguration, string>
        {
            Names = new string[] { "-a", "--algorithm" },
            Description = "Set algorithm to use",
            Action = (storage, value) => { storage.algorithm = value; },
            Converter = ConverterFactory.CreateStringConverter(),
        };

        var inputFileArg = new Argument<ExampleConfiguration, string>
        {
            ValuePlaceholder = "input file",
            Description = "File to process",
            Multiplicity = new ArgumentMultiplicity.SpecificCount(1, true),
            Action = (storage, value) => { storage.inputFile = value; },
            Converter = ConverterFactory.CreateStringConverter()
        };
        parser.AddArgument(inputFileArg);

        parser.AddOption(algorithmOption);

        // Now we would call `parser.Parse(args);` and we our config instance would be
        // populated with the values from the command line (or an exception is thrown)
    }

}
