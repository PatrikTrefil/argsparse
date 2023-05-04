using Argparse;
using System.Collections.Generic;
using System.IO;

namespace Numactl;

class NumaParser
{

    NumaCtlArgs args = new NumaCtlArgs();

    private Parser<NumaCtlArgs> parser = new Parser<NumaCtlArgs>(() => new NumaCtlArgs())
    {
        Names = new string[] { "numactl" },
        Description = "Run a program under control of numactl.",
    };

    public NumaParser()
    {
        parser.Run = (result, _) => args = result;
        Configure();
    }

    public NumaCtlArgs Parse(string[] rawArgs)
    {
        args = new NumaCtlArgs();

        parser.ParseAndRun(rawArgs);
        args.AssertValid();

        return args;
    }

    public void PrintHelpTo(TextWriter tw)
    {
        var formatter = new DefaultHelpFormatter<NumaCtlArgs>();
        formatter.PrintHelp(parser, tw);
    }

    public void PrintHelp()
    {
        PrintHelpTo(System.Console.Out);
    }

    private void Configure()
    {
        parser.AddFlag(new Flag<NumaCtlArgs>
        {
            Names = new string[] { "-h", "--help" },
            Description = "Show help for the program.",
            Action = (x) => x.Help = true,
        });

        parser.AddOption(new Option<NumaCtlArgs, List<int>>
        {
            Names = new string[] { "-i", "--interleave" },
            Description = "Interleave memory allocation across given nodes.",
            Action = (x, v) => x.Interleave = v,
            Converter = ConverterFactory.CreateListConverter(int.Parse),
        });

        parser.AddOption(new Option<NumaCtlArgs, int>
        {
            Names = new string[] { "-p", "--preferred" },
            Description = "Prefer memory allocations from given node.",
            Action = (x, v) => x.Preferred = v,
            Converter = ConverterFactory.CreateIntConverter()
        });

        parser.AddOption(new Option<NumaCtlArgs, List<int>>
        {
            Names = new string[] { "-m", "--membind" },
            Description = "Allocate memory from given nodes only.",
            Action = (x, v) => x.MemBind = v,
            Converter = ConverterFactory.CreateListConverter(int.Parse),
        });

        parser.AddOption(new Option<NumaCtlArgs, List<int>>
        {
            Names = new string[] { "-C", "--physcpubind" },
            Description = "Run on given CPUs only.",
            Action = (x, v) => x.PhysCpuBind = v,
            Converter = ConverterFactory.CreateListConverter(int.Parse),
        });

        parser.AddFlag(new Flag<NumaCtlArgs>
        {
            Names = new string[] { "-S", "--show" },
            Description = "Show current NUMA policy.",
            Action = (x) => x.Show = true,
        });

        parser.AddFlag(new Flag<NumaCtlArgs>
        {
            Names = new string[] { "-H", "--hardware" },
            Description = "Print hardware configuration.",
            Action = (x) => x.Hardware = true,
        });

        parser.AddArgument(new Argument<NumaCtlArgs, string>
        {
            Description = "Command to be executed on NUMA architecture.",
            ValuePlaceholder = "command",
            Multiplicity = new ArgumentMultiplicity.SpecificCount(1, IsRequired: false),
            Action = (x, v) => x.Command = v,
            Converter = ConverterFactory.CreateStringConverter()
        });

        parser.AddArgument(new Argument<NumaCtlArgs, List<string>>
        {
            Description = "Arguments of the specified command.",
            ValuePlaceholder = "args",
            Multiplicity = new ArgumentMultiplicity.AllThatFollow(),
            Action = (x, v) => x.CommandArgs = v,
            Converter = ConverterFactory.CreateListConverter(ConverterFactory.CreateStringConverter())
        });
    }
}
