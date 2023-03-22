using System.Collections.Generic;

namespace argparse.Examples;

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

        var formatOption = OptionFactory<TimeCommandConfiguration>.CreateStringOption() with
        {
            Names = new string[] { "-f", "--format" },
            Description = "Specify output format, possibly overriding the format specified in the environment variable TIME.",
            ValuePlaceHolder = "FORMAT",
            Action = (storage, value) =>
            {
                storage.format = value;
            }
        };
        parser.AddOption(formatOption);

        var outputFileOption = OptionFactory<TimeCommandConfiguration>.CreateStringOption() with
        {
            Names = new string[] { "-o", "--output" },
            Description = "Do not send the results to stderr, but overwrite the specified file.",
            ValuePlaceHolder = "FILE"
        };
        parser.AddOption(outputFileOption);


        // if we needed special parsing for different kinds of commands, we could use subparsers
        parser.AddArgument(ArgumentFactory<TimeCommandConfiguration>.CreateStringArgument() with
        {
            Multiplicity = new ArgumentMultiplicity.SpecificCount(1, true),
            Action = (storage, value) => { storage.command = value; },
            Name = "command",
            ValuePlaceholder = "command"
        });

        parser.AddArgument(ArgumentFactory<TimeCommandConfiguration>.CreateStringArgument() with
        {
            Multiplicity = new ArgumentMultiplicity.AllThatFollow(),
            Action = (storage, value) => { storage.arguments.Add(value); },
            Name = "arg-name"
        });


        // Now we would call `parser.Parse(args);` and we our config instance would be
        // populated with the values from the command line (or an exception is thrown)

        parser.PrintHelp();
    }
}
