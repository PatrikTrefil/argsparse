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
            System.Console.WriteLine("Invalid program arguments:");
            System.Console.WriteLine(e.Message + "\n");
            System.Console.WriteLine("  ...see numactl --help");
            return;
        }

        if (args.HelpMode())
        {
            parser.PrintHelp();
        }
        else if (args.ShowMode())
        {
            // show mode logic
        }
        else if (args.HardwareMode())
        {
            // hardware mode logic
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
