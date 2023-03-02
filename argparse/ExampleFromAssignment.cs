using System;

namespace Argsparse.Examples;

class ExampleFromAssignment
{

    //    time[options] command[arguments...]

    //GNU Options
    //    -f FORMAT, --format=FORMAT
    //           Specify output format, possibly overriding the format specified
    //           in the environment variable TIME.
    //    -p, --portability
    //           Use the portable output format.
    //    -o FILE, --output= FILE
    //           Do not send the results to stderr, but overwrite the specified file.
    //    -a, --append
    //           (Used together with -o.) Do not overwrite but append.
    //    -v, --verbose
    //           Give very verbose output about all the program knows about.

    //GNU Standard Options
    //    --help Print a usage message on standard output and exit successfully.
    //    -V, --version
    //           Print version information on standard output, then exit successfully.
    //    --     Terminate option list.

    class TimeApp : ParserContext
    {
        public static string Version { get => "1.2.3a"; }
        
        public string? Format { get; set; }
        public bool UsePortableOutput { get; set; }
        public string Filename { get; set; }
        public bool UseAppend { get; set; }

        public bool Verbose { get; set; }

    }


    public void Run()
    {
        var parser = Parser<TimeApp>.Create()
            .WithOption("-f", "--format")
            .WithHelpText(" Specify output format, possibly overriding the format " +
            "specified in the environment variable TIME.")
            .WithValue()
            .Does((c, f) => c.Format = f).And()
            .WithOption("-p", "--portability")
            .Does(c => c.UsePortableOutput = true).And()
            .WithOption("-o", "--output")
            .WithValue()
            .Does((c, file) => c.Filename = file)
            .And()
            .WithOption("-a", "--append")
            .Does(c => c.UseAppend = true)
            .And()
            .WithOption("-v", "--verbose")
            .Does(c => c.Verbose = true)
            .And()
            .WithOption("-V", "--version")
            .Does(c => c.Console.Println(TimeApp.Version))
            .And();

    }

}