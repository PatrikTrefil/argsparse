namespace Argsparse;

public abstract class ParserContext
{
   public ConsoleContext Console { get; }

}

public class ConsoleContext
{

    public void ShowHelp()
    {

    }

    public void ShowUsage() => ShowHelp();

    public void Println(string message)
    {
    }

    public void PrintError(string message)
    {

    }
}
