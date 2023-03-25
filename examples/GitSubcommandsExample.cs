using System;

namespace Argparse.Examples;

internal static class GitSubcommandsExample {

    record GitConfig {
        public bool Help = false;
        public bool Version = false;
    }

    record GitCloneConfig : GitConfig {
        public string? Repository;
    }

    record GitPushConfig : GitConfig {
        public string? Repository;
        public string? BranchOrRefspec;

        public bool Tags = false;
    }

    record GitLogConfig : GitConfig {
        public bool prettyPrint = false;
    }

    static readonly ArgumentMultiplicity ZeroOrOne = new ArgumentMultiplicity.SpecificCount(Number: 1, IsRequired: false);

 

    public static void Run(string[] arguments)
    {
        Parser<GitConfig> GitParser = ConfigureParser();
        GitParser.ParseAndRun(arguments);
    }

    private static Parser<GitConfig> ConfigureParser()
    {
        var helpFormatter = new DefaultHelpFormatter<GitConfig>();

        static Flag<T> MakeHelpFlag<T>() where T : GitConfig
        {
            return new Flag<T>()
            {
                Names = new string[] { "-h", "--help" },
                Description = "Print help",
                Action = (config) => { config.Help = true; }
            };
        }
        /// PUSH

        var PushParser = new Parser<GitPushConfig>(() => new GitPushConfig())
        {
            Name = "push",
            Description = "Pushes to a remote repository",
            Run = (config, parser) =>
            {
                if (config.Help)
                {
                    helpFormatter.PrintHelp(parser, Console.Out);
                    return;
                }
                Console.WriteLine("Pushing to remote repository");
                if (config.Tags) Console.WriteLine("Pushing tags");
                if (config.Repository is not null) Console.WriteLine("Pushing to repository: " + config.Repository);
                if (config.BranchOrRefspec is not null) Console.WriteLine("Pushing branch or refspec: " + config.BranchOrRefspec);
            }
        };
        PushParser.AddFlag(MakeHelpFlag<GitPushConfig>());
        PushParser.AddFlag(new Flag<GitPushConfig>()
        {
            Names = new string[] { "-t", "--tags" },
            Description = "Pushes tags",
            Action = (config) => { config.Tags = true; }
        });

        PushParser.AddArgument(new Argument<GitPushConfig, string>()
        {
            ValuePlaceholder = "repository",
            Description = "Repository to push to",
            Action = (config, value) => { config.Repository = value; },
            Converter = ConverterFactory.CreateStringConverter(),
            Multiplicity = ZeroOrOne
        });

        PushParser.AddArgument(new Argument<GitPushConfig, string>()
        {
            ValuePlaceholder = "branch or refspec",
            Description = "Branch or refspec to push",
            Action = (config, value) => { config.BranchOrRefspec = value; },
            Converter = ConverterFactory.CreateStringConverter(),
            Multiplicity = ZeroOrOne
        });

        /// CLONE

        var CloneParser = new Parser<GitCloneConfig>(() => new GitCloneConfig())
        {
            Name = "clone",
            Description = "Clones a remote repository",
            Run = (config, parser) =>
            {
                if (config.Help)
                {
                    helpFormatter.PrintHelp(parser, Console.Out);
                    return;
                }
                Console.WriteLine("Cloning remote repository");
                if (config.Repository is not null) Console.WriteLine("Cloning repository: " + config.Repository);
            }
        };

        CloneParser.AddFlag(MakeHelpFlag<GitCloneConfig>());

        CloneParser.AddArgument(new Argument<GitCloneConfig, string>()
        {
            ValuePlaceholder = "repository",
            Description = "Repository to clone",
            Action = (config, value) => { config.Repository = value; },
            Converter = ConverterFactory.CreateStringConverter(),
        });

        /// LOG

        var LogParser = new Parser<GitLogConfig>(() => new GitLogConfig())
        {
            Name = "log",
            Description = "Shows the commit log",
            Run = (config, parser) =>
            {
                if (config.Help)
                {
                    helpFormatter.PrintHelp(parser, Console.Out);
                    return;
                }
                Console.WriteLine("Showing commit log");
                if (config.prettyPrint) Console.WriteLine("Pretty printing. Hi, pretty :)");
            }
        };

        LogParser.AddFlag(MakeHelpFlag<GitLogConfig>());

        LogParser.AddFlag(new Flag<GitLogConfig>()
        {
            Names = new string[] { "-p", "--pretty" },
            Description = "Pretty prints the log",
            Action = (config) => { config.prettyPrint = true; }
        });

        /// GIT

        var GitParser = new Parser<GitConfig>(() => new GitConfig())
        {
            Name = "git",
            Description = "A git clone written in C#",
            Run = (config, parser) =>
            {
                if (config.Help)
                {
                    helpFormatter.PrintHelp(parser, Console.Out);
                    return;
                }
                Console.WriteLine("Running git");
                if (config.Help) Console.WriteLine();
                if (config.Version) Console.WriteLine("version XXX");
            }
        };

        GitParser.AddFlag(MakeHelpFlag<GitConfig>());

        GitParser.AddFlag(new Flag<GitConfig>()
        {
            Names = new string[] { "-v", "--version" },
            Description = "Shows the version",
            Action = (config) => { config.Version = true; }
        });

        GitParser.AddSubparser("push", PushParser);
        GitParser.AddSubparser("clone", CloneParser);
        GitParser.AddSubparser("log", LogParser);
        return GitParser;
    }
}