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
        // If the user does not mind null values in the config, they can just provide
        // the config object directly
        // 
        var config = new ExampleConfiguration();

        var parser =  Parser<ExampleConfiguration>.Create(config)
            .WithName("program")
            .WithDescription("Program description");

        var helpOption = (Flag<ExampleConfiguration>.Create())
            .WithNames("-h", "--help")
            .WithDescription("Show help")
            .WithAction((storage) => { storage.help = true; });
        parser.AddFlag(helpOption);

        var algorithmOption = OptionFactory.CreateStringOption<ExampleConfiguration>()
            .WithNames("-a", "--algorithm")
            .WithDescription("Set algorithm to use")
            .WithAction((storage, value) => { storage.algorithm = value; });



        parser.Parse(args);

    }
}