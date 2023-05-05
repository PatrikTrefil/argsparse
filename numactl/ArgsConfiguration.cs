using Argparse;
using System.Collections.Generic;
using System.IO;

namespace Numactl;

class NumaParser
{

    NumaCtlArgs args = new();

    private Parser<NumaCtlArgs> parser;
    public NumaParser()
    {
        parser = new Parser<NumaCtlArgs>(args)
        {
            Names = new() { "numactl" },
            Description = "Run a program under control of numactl.",
            Run = (result, _) => args = result
        };

        Configure();
    }

    public NumaCtlArgs Parse(string[] rawArgs)
    {
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
            Names = new() { "-h", "--help" },
            Description = "Show help for the program.",
            Action = (x) => x.Help = true,
        });

        parser.AddOption(new Option<NumaCtlArgs, List<int>>
        {
            Names = new() { "-i", "--interleave" },
            Description = "Interleave memory allocation across given nodes.",
            Action = (x, v) => x.Interleave = v,
            Converter = ConverterFactory.CreateListConverter(int.Parse),
        });

        parser.AddOption(new Option<NumaCtlArgs, int>
        {
            Names = new() { "-p", "--preferred" },
            Description = "Prefer memory allocations from given node.",
            Action = (x, v) => x.Preferred = v,
            Converter = ConverterFactory.CreateIntConverter()
        });

        parser.AddOption(new Option<NumaCtlArgs, List<int>>
        {
            Names = new() { "-m", "--membind" },
            Description = "Allocate memory from given nodes only.",
            Action = (x, v) => x.MemBind = v,
            Converter = ConverterFactory.CreateListConverter(int.Parse),
        });

        parser.AddOption(new Option<NumaCtlArgs, List<int>>
        {
            Names = new() { "-C", "--physcpubind" },
            Description = "Run on given CPUs only.",
            Action = (x, v) => x.PhysCpuBind = v,
            Converter = ConverterFactory.CreateListConverter(int.Parse),
        });

        parser.AddFlag(new Flag<NumaCtlArgs>
        {
            Names = new() { "-S", "--show" },
            Description = "Show current NUMA policy.",
            Action = (x) => x.Show = true,
        });

        parser.AddFlag(new Flag<NumaCtlArgs>
        {
            Names = new() { "-H", "--hardware" },
            Description = "Print hardware configuration.",
            Action = (x) => x.Hardware = true,
        });

        parser.AddArgument(new Argument<NumaCtlArgs, string>
        {
            Description = "Command and arguments of the specified command.",
            ValuePlaceholder = "args",
            Multiplicity = new ArgumentMultiplicity.AllThatFollow(),
            Action = (x, v) =>
            {
                if (x.Command is null)
                {
                    x.Command = v;
                    x.CommandArgs = new();
                }
                else
                    x.CommandArgs.Add(v);
            },
            Converter = ConverterFactory.CreateStringConverter()
        });
    }
}
