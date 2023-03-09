namespace Argsparse.Examples;

class Program
{

    internal class ExampleConfiguration
    {
        public string? algorithm;
        public bool help = false;
    }

    static void Main(string[] args)
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
}