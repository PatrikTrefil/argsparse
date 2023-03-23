using System.Collections.Generic;

namespace Argparse.Examples;

internal class TimeExample
{
    record TimeCommandConfiguration
    {
        public bool help = false;
        public bool portability = false;
        public bool version = false;
        public bool append = false;
        public bool verbose = false;
        public string? format;
        public List<string> arguments = new();
        public string? command;
        public string? outputFile;
    }

    public static void Run(string[] args)
    {
        var config = new TimeCommandConfiguration();

        var parser = new Parser<TimeCommandConfiguration>(config)
        {
            Name = "time",
        };

        var helpFlag = new Flag<TimeCommandConfiguration>()
        {
            Names = new string[] { "-h", "--help" },
            Description = "Print a usage message on standard output and exit successfully.",
            Action = (storage) => { storage.help = true; }
        };

        parser.AddFlag(helpFlag);

        var versionFlag = new Flag<TimeCommandConfiguration>()
        {
            Names = new string[] { "-V", "--version" },
            Description = "Print version information on standard output, then exit successfully.",
            Action = (storage) => { storage.version = true; }
        };
        parser.AddFlag(versionFlag);

        var appendFlag = new Flag<TimeCommandConfiguration>()
        {
            Names = new string[] { "-a", "--append" },
            Description = "(Used together with -o.) Do not overwrite but append.",
            Action = (storage) => { storage.append = true; }
        };
        parser.AddFlag(appendFlag);

        var verboseFlag = new Flag<TimeCommandConfiguration>()
        {
            Names = new string[] { "-v", "--verbose " },
            Description = "Give very verbose output about all the program knows about.",
            Action = (storage) => { storage.verbose = true; }
        };
        parser.AddFlag(verboseFlag);

        var portabilityFlag = new Flag<TimeCommandConfiguration>()
        {
            Names = new string[] { "-p", "--portability" },
            Description = "Use the portable output format.",
            Action = (storage) => { storage.portability = true; }
        };

        parser.AddFlag(portabilityFlag);

        var formatOption = new Option<TimeCommandConfiguration, string>
        {
            Names = new string[] { "-f", "--format" },
            Description = "Specify output format, possibly overriding the format specified in the environment variable TIME.",
            ValuePlaceHolder = "FORMAT",
            Action = (storage, value) =>
            {
                storage.format = value;
            },
            Converter = ConverterFactory.CreateStringConverter()
        };
        parser.AddOption(formatOption);

        var outputFileOption = new Option<TimeCommandConfiguration, string>
        {
            Names = new string[] { "-o", "--output" },
            Description = "Do not send the results to stderr, but overwrite the specified file.",
            ValuePlaceHolder = "FILE",
            Converter = ConverterFactory.CreateStringConverter(),
            Action = (storage, value) =>
            {
                storage.outputFile = value;
            }
        };
        parser.AddOption(outputFileOption);


        // if we needed special parsing for different kinds of commands, we could use subparsers
        parser.AddArgument(new Argument<TimeCommandConfiguration, string>
        {
            Multiplicity = new ArgumentMultiplicity.SpecificCount(1, true),
            Action = (storage, value) => { storage.command = value; },
            Name = "command",
            ValuePlaceholder = "command",
            Converter = ConverterFactory.CreateStringConverter(),
            Description = "command to run"
        });

        parser.AddArgument(new Argument<TimeCommandConfiguration, string>
        {
            Multiplicity = new ArgumentMultiplicity.AllThatFollow(),
            Action = (storage, value) => { storage.arguments.Add(value); },
            Name = "arg-name",
            Converter = ConverterFactory.CreateStringConverter(),
            Description = "arg desc"
        });


        // Now we would call `parser.Parse(args);` and we our config instance would be
        // populated with the values from the command line (or an exception is thrown)

        parser.PrintHelp();
    }
}
