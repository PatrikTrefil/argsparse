using System;

namespace Numactl;

public class Program
{
    public static void Main(string[] rawArgs)
    {
        var parser = new NumaParser();
        NumaCtlArgs args;
        try
        {
            args = parser.Parse(rawArgs);
        }
        catch (InvalidProgramArgumentsException e)
        {
            Console.WriteLine("Invalid program arguments:");
            Console.WriteLine(e.Message + "\n");
            Console.WriteLine("  ...see numactl --help");
            return;
        }

        if (args.HelpMode())
        {
            parser.PrintHelp();
        }
        else if (args.ShowMode())
        {
            Console.WriteLine("Show mode");
            // show mode logic ...
        }
        else if (args.HardwareMode())
        {
            Console.WriteLine("Hardware mode");
            // hardware mode logic ...
        }
        else if (args.ExecutionMode())
        {
            // execution mode logic
            var policyPrinter = new PolicyPrinter(Console.Out);
            policyPrinter.PrintPolicyFor(args);
        }
        else
            throw new NotImplementedException("The program mode has not been implemented.");

    }
}
