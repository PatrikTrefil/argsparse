using System;
using System.Collections.Generic;
using System.Linq;

namespace Argparse.Examples;

internal static class NumactlExample
{
    public static class Converter
    {
        public static int ParseIntWithConstraint(string value, int min, int max)
        {
            var result = int.Parse(value);
            if (result < min || result > max)
            {
                throw new ArgumentException($"Invalid value {value} must be in range {min}-{max}");
            }

            return result;
        }

        public static List<int> ParseNumactlIntList(string value, int min, int max)
        {
            var result = new List<int>();
            var parts = value.Split(',');

            if (value == "all")
            {
                for (var i = min; i <= max; i++)
                {
                    result.Add(i);
                }

                return result;
            }

            foreach (var part in parts)
            {
                if (part.Contains('-'))
                {
                    var range = part.Split('-');
                    if (range.Length != 2)
                    {
                        throw new ArgumentException($"Invalid range {part}");
                    }

                    var start = int.Parse(range[0]);
                    var end = int.Parse(range[1]);
                    if (start < min || start > max)
                    {
                        throw new ArgumentException($"Invalid range start {start}, must be in range {min}-{max}");
                    }

                    if (end < min || end > max)
                    {
                        throw new ArgumentException($"Invalid range end {end}, must be in range {min}-{max}");
                    }

                    if (start > end)
                    {
                        throw new ArgumentException($"Invalid range {part}");
                    }

                    for (var i = start; i <= end; i++)
                    {
                        result.Add(i);
                    }
                }
                else
                {
                    var number = int.Parse(part);
                    if (number < min || number > max)
                    {
                        throw new ArgumentException($"Invalid number {number}, must be in range {min}-{max}");
                    }

                    result.Add(number);
                }
            }

            result.Sort();
            return result.Distinct().ToList();
        }
    }

    public record NumactlConfiguration
    {
        private const int MinNumaNode = 0;
        private const int MaxNumaNode = 3;

        private const int MinCpuID = 0;
        private const int MaxCpuID = 31;

        public bool Help = false;
        public bool Hardware = false;
        public bool Show = false;

        public int Preferred = -1;

        public List<int> CpuNodeBind = new();
        public List<int> MemoryBind = new();
        public List<int> Interleave = new();

        public string Command = string.Empty;
        public List<string> CommandArgs = new();

        public static int ParseNumaNode(string value) =>
            Converter.ParseIntWithConstraint(value, MinNumaNode, MaxNumaNode);

        public static List<int> ParseCpuIDList(string value) =>
            Converter.ParseNumactlIntList(value, MinCpuID, MaxCpuID);

        public static List<int> ParseNumaNodeList(string value) =>
            Converter.ParseNumactlIntList(value, MinNumaNode, MaxNumaNode);

        private bool IsShowAlone()
        {
            return Show && !Hardware && CpuNodeBind.Count == 0 && MemoryBind.Count == 0 && Interleave.Count == 0 &&
                   Command == string.Empty;
        }

        private bool IsHardwareAlone()
        {
            return Hardware && !Show && CpuNodeBind.Count == 0 && MemoryBind.Count == 0 && Interleave.Count == 0 &&
                   Command == string.Empty;
        }

        public void CheckForConflictingInfoOptions()
        {
            if (Show && !IsShowAlone())
                throw new ArgumentException("Cannot use --show with other options");
            if (Hardware && !IsHardwareAlone())
                throw new ArgumentException("Cannot use --hardware with other options");
        }

        public void CheckForConflictingRunOptions()
        {
            if (Preferred != -1 && (MemoryBind.Count > 0 || Interleave.Count > 0))
                throw new ArgumentException("Cannot use --preferred with --membind or --interleave");
            if (MemoryBind.Count > 0 && Interleave.Count > 0)
                throw new ArgumentException("Cannot use --membind with --interleave");
            if (Command == string.Empty)
                throw new ArgumentException("No command specified");
        }
    }

    private static void Configuration(Parser<NumactlConfiguration> parser)
    {
        parser.AddFlag(new Flag<NumactlConfiguration>
        {
            Names = new[] { "-h", "--help" },
            Description = "Show help",
            Action = storage => { storage.Help = true; }
        });

        parser.AddFlag(new Flag<NumactlConfiguration>
        {
            Names = new[] { "-H", "--hardware" },
            Description = "Show hardware information",
            Action = storage => { storage.Hardware = true; }
        });

        parser.AddFlag(new Flag<NumactlConfiguration>
        {
            Names = new[] { "-S", "--show" },
            Description = "Show current NUMA policy",
            Action = storage => { storage.Show = true; }
        });

        parser.AddOption(new Option<NumactlConfiguration, List<int>>
        {
            Names = new[] { "-C", "--physcpubind" },
            Description = "Run on given CPUs only",
            Action = (storage, value) => { storage.CpuNodeBind = value; },
            ValuePlaceHolder = "<cpus>",
            Converter = NumactlConfiguration.ParseCpuIDList
        });

        parser.AddOption(new Option<NumactlConfiguration, List<int>>
        {
            Names = new[] { "-i", "--interleave" },
            Description = "Interleave memory allocation across given nodes",
            Action = (storage, value) => { storage.Interleave = value; },
            ValuePlaceHolder = "<nodes>",
            Converter = NumactlConfiguration.ParseNumaNodeList
        });

        parser.AddOption(new Option<NumactlConfiguration, List<int>>
        {
            Names = new[] { "-m", "--membind" },
            Description = "Allocate memory from given nodes only",
            Action = (storage, value) => { storage.MemoryBind = value; },
            ValuePlaceHolder = "<nodes>",
            Converter = NumactlConfiguration.ParseNumaNodeList
        });


        parser.AddOption(new Option<NumactlConfiguration, int>
        {
            Names = new[] { "-p", "--preferred" },
            Description = "Prefer memory allocations from given node",
            Action = (storage, value) => { storage.Preferred = value; },
            ValuePlaceHolder = "<node>",
            Converter = NumactlConfiguration.ParseNumaNode
        });

        parser.AddArgument(new Argument<NumactlConfiguration, string>
        {
            Name = "command",
            Description = "Command to run",
            Action = (storage, value) => { storage.Command = value; },
            Multiplicity = new ArgumentMultiplicity.SpecificCount(1, false),
            Converter = ConverterFactory.CreateStringConverter()
        });

        parser.AddArgument(new Argument<NumactlConfiguration, string>
        {
            Name = "command arguments",
            Description = "Arguments for command",
            Action = (storage, value) => { storage.CommandArgs.Add(value); },
            Multiplicity = new ArgumentMultiplicity.AllThatFollow(),
            Converter = ConverterFactory.CreateStringConverter()
        });
    }

    public static void Run(string[] arguments)
    {

        var config = new NumactlConfiguration();
        var parser = new Parser<NumactlConfiguration>(config)
        {
            Names = new string[] {"numactl"},
            Description = "numactl description"
        };

        Configuration(parser);

        try
        {
            parser.Parse(arguments);
        }
        catch (Exception e) when (e is ArgumentException or ParserConversionException or ParserRuntimeException)
        {
            Console.WriteLine(e.Message);
            return;
        }

        if (config.Help || arguments.Length == 0)
        {
            parser.PrintHelp();
            return;
        }

        try
        {
            config.CheckForConflictingInfoOptions();
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.Message);
            return;
        }

        if (config.Hardware)
        {
            Console.WriteLine(DummyMessages.Hardware);
            return;
        }

        if (config.Show)
        {
            Console.WriteLine(DummyMessages.Show);
            return;
        }


        try
        {
            config.CheckForConflictingRunOptions();
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.Message);
            return;
        }

        RunCommand(config);
    }

    private static void RunCommand(NumactlConfiguration config)
    {
        var cpuNodeBind = config.CpuNodeBind.Count == 0 ? string.Join(',', config.CpuNodeBind) : "default";
        var nodeMemoryPolicy = (config.MemoryBind.Count, config.Interleave.Count, config.Preferred) switch
        {
            (0, 0, -1) => "default",
            (0, 0, _) => $"preferred: {config.Preferred}",
            (0, _, 0) => $"interleave: {string.Join(',', config.Interleave)}",
            (_, 0, 0) => $"membind: {string.Join(',', config.MemoryBind)}",
            (_, _, _) => throw new ArgumentException("Invalid option combination")
        };

        Console.WriteLine($"Running command {config.Command} {string.Join(' ', config.CommandArgs)}");
        Console.WriteLine($"CpuNodeBind: {cpuNodeBind}");
        Console.WriteLine($"MemoryPolicy: {nodeMemoryPolicy}");
    }
}

public static class DummyMessages
{
    public const string Hardware =
        @"available: 2 nodes (0-1)
node 0 cpus: 0 2 4 6 8 10 12 14 16 18 20 22
node 0 size: 24189 MB
node 0 free: 18796 MB
node 1 cpus: 1 3 5 7 9 11 13 15 17 19 21 23
node 1 size: 24088 MB
node 1 free: 16810 MB
node distances:
node   0   1
       0:  10  20
       1:  20  10";

    public const string Show =
        @"policy: default
preferred node: current
physcpubind: 0 1 2 3 4 5 6 7 8
cpubind: 0 1
nodebind: 0 1
membind: 0 1";
}