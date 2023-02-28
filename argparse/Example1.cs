namespace Argsparse.Examples;

using Argsparse;

class Example1
{
    class WelcomeCommand : SubcommandParserContext
    {
        public string WelcomeMessage { get; set; }

        private bool useBang = false;
        public void UseExclamationMark() => useBang = true;

        public void run()
        {
            var message = WelcomeMessage + (useBang ? "!" : "");

            this.Console.Println(message);
        }
    }

    class PartingCommand : SubcommandParserContext
    {
        public string PartingMessage { get; set; } = "Good bye my friend";

    }

    static void run()
    {

        var parser = Parser.Create()
              .WithOption("--myhelp", "--magic").And()
              .WithArgument<string>("my_positional_argument")
              .Optional()
              .And()
              .WithCommand<WelcomeCommand>("welcome")
              .WithOption("-m", "--message")
              .WithValue<string>()
              .Default("Hello")
              .Does((command, value) => command.WelcomeMessage = value)
              .And()
              .WithOption("--fiercely")
              .Does(command => command.UseExclamationMark())
              .And().Furthermore() // Ugly ?
              .WithCommand<PartingCommand>("saybye")
              .Does( (c) =>
              {
                  c.Console.Println(c.PartingMessage);
              });

    }
}
