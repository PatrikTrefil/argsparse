using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Numactl;
class PolicyPrinter
{
    private readonly TextWriter output;
    private const string Undefined = "<undefined>";
    public PolicyPrinter(TextWriter output)
    {
        this.output = output;
    }

    public void PrintPolicyFor(NumaCtlArgs args)
    {
        Print("help", args.Help);
        Print("interleave", args.Interleave);
        Print("preferred", args.Preferred);
        Print("memory bind", args.MemBind);
        Print("physical cpu bind", args.PhysCpuBind);
        Print("show", args.Show);
        Print("hardware", args.Hardware);
        Print("command", args.Command);
        Print("command args", args.CommandArgs);
    }

    private void Print(string label, object? value)
    {
        if (value?.GetType() == typeof(List<int>))
            PrintListOfInts(label, (List<int>?)value);
        // other option can be added
        else
            PrintObject(label, value);
    }

    private void PrintListOfInts(string label, List<int>? list)
    {
        if (list == null)
        {
            output.WriteLine(label + ":\t" + Undefined);
            return;
        }

        var valueString = string.Join(",", list);

        output.WriteLine(label + ":\t" + valueString);
    }

    private void PrintObject(string label, object? value)
    {
        output.WriteLine($"{label}:\t{value?.ToString() ?? Undefined}");
    }

}