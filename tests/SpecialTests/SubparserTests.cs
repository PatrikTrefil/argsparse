namespace TestsArgparseAPI.SpecialTests
{

    record RootCommandConfig
    {
        public bool help = false;
    }
    record SubCommandConfig
    {
        public bool help = false;
        public int? number;
        public bool flag;
    }

    record SecondSubCommandConfig
    {
        public bool help = false;
        public int? number;
    }

    /// <summary>
    /// Classes and config records are partially taken from ComplexExample.cs.
    /// 
    /// SubparserTests class provides the set of tests for subparser functionality.
    /// The set consists of:
    /// - one configuration test
    /// - parsing tests with one subparser
    /// - parsing tests with two subparser
    /// - parsing tests with nested subparsers
    /// </summary>
    internal class SubparserTests
    {
        private Parser<RootCommandConfig> toplevelParser;
        private Parser<SubCommandConfig> subcommandParser;
        private Parser<SecondSubCommandConfig> secondsubcommandParser;
        private Parser<SecondSubCommandConfig> nestedsubcommandParser;

        /// <summary>
        /// In the setup three parsers are set with contexts and required arguments and flags.
        /// Adding of subparsers is then done directly in the tests to simulate different use cases.
        /// </summary>
        [SetUp]
        public void TestSetup()
        {
            // toplevel parser definition
            toplevelParser = new Parser<RootCommandConfig>(
            () => new RootCommandConfig()
            )
            {
                Names = new() { "program" },
                Description = "My description",
                Run = (c, _) => { Console.WriteLine("I will run after my config is ready"); }
            };

            toplevelParser.AddFlag(new Flag<RootCommandConfig>
            {
                Names = new() { "-h", "--help" },
                Description = "Print help",
                Action = (c) => { c.help = true; }
            });

            subcommandParser = new Parser<SubCommandConfig>(
                () => new SubCommandConfig()
                )
            {
                Names = new() { "subcommand" },
                Description = "My subcommand description",
                Run = (c, _) => { Console.WriteLine("I will run after my config is ready"); }
            };

            // subcommand parser definition
            subcommandParser.AddFlag(new Flag<SubCommandConfig>()
            {
                Names = new() { "-h", "--help" },
                Description = "Print help",
                Action = (c) => { c.help = true; }
            });

            var numberArg = new Argument<SubCommandConfig, int>
            {
                ValuePlaceholder = "Number",
                Description = "Number description",
                Action = (storage, value) => { storage.number = value; },
                Converter = ConverterFactory.CreateIntConverter()
            };

            subcommandParser.AddArgument(numberArg);

            // second subcommand definition
            secondsubcommandParser = new Parser<SecondSubCommandConfig>(
                () => new SecondSubCommandConfig()
                )
            {
                Names = new() { "secondsubcommand" },
                Description = "My subcommand description",
                Run = (c, _) => { Console.WriteLine("I will run after my config is ready"); }
            };

            secondsubcommandParser.AddFlag(new Flag<SecondSubCommandConfig>()
            {
                Names = new() { "-h", "--help" },
                Description = "Print help",
                Action = (c) => { c.help = true; }
            });

            secondsubcommandParser.AddArgument(new Argument<SecondSubCommandConfig, int>()
            {
                ValuePlaceholder = "Number",
                Description = "Number description",
                Action = (storage, value) => { storage.number = value; },
                Converter = ConverterFactory.CreateIntConverter()
            });

            // nested subcommand definition
            nestedsubcommandParser = new Parser<SecondSubCommandConfig>(
                () => new SecondSubCommandConfig()
                )
            {
                Names = new() { "nestedcommand" },
                Description = "My subcommand description",
                Run = (c, _) => { Console.WriteLine("I will run after my config is ready"); }
            };

            nestedsubcommandParser.AddFlag(new Flag<SecondSubCommandConfig>()
            {
                Names = new() { "-h", "--help" },
                Description = "Print help",
                Action = (c) => { c.help = true; }
            });

            nestedsubcommandParser.AddArgument(new Argument<SecondSubCommandConfig, int>()
            {
                ValuePlaceholder = "Number",
                Description = "Number description",
                Action = (storage, value) => { storage.number = value; },
                Converter = ConverterFactory.CreateIntConverter()
            });
        }

        /// <summary>
        /// Configuration test.
        /// </summary>
        [Test]
        public void SubparserAddedSuccesfully()
        {
            toplevelParser.AddSubparser(subcommandParser);

            Assert.That(toplevelParser.SubParsers.Count, Is.EqualTo(1));
        }

        [Test]
        [TestCase("subcommand --help 1")]
        [TestCase("subcommand -h 1")]
        public void SubparserCalledSuccesfully(string input)
        {
            var splittedInput = input.Split(' ');

            toplevelParser.AddSubparser(subcommandParser);
            toplevelParser.Parse(splittedInput);

            Assert.Multiple(() =>
            {
                Assert.That(subcommandParser.Config.help, Is.True);
                Assert.That(toplevelParser.Config, Is.Null);
            });
        }

        [Test]
        [TestCase("-h")]
        public void HelpColledWithoutSubcommandSetOnlyForRootParser(string input)
        {
            var splittedInput = input.Split(' ');

            toplevelParser.AddSubparser(subcommandParser);
            toplevelParser.Parse(splittedInput);

            Assert.Multiple(() =>
            {
                Assert.That(subcommandParser.Config, Is.Null);
                Assert.That(toplevelParser.Config.help, Is.True);
            });
        }

        [Test]
        [TestCase("subcommand 5 secondcommand 2")]
        [TestCase("secondcommand 2 subcommand 5")]
        [TestCase("subcommand 5 secondcommand")]
        [TestCase("subcommand secondcommand")]
        [TestCase("secondcommand subcommand 2")]
        public void InvalidMultipleSubcommandInputRaisesException(string input)
        {
            var splittedInput = input.Split(' ');

            toplevelParser.AddSubparser(subcommandParser);
            toplevelParser.AddSubparser(secondsubcommandParser);

            Assert.That(() => toplevelParser.Parse(splittedInput),
                Throws.InstanceOf<ParserRuntimeException>());
        }

        /// <summary>
        /// Testing possible wrong inputs leading to errors for parser
        /// with two levels of subparsers.
        /// </summary>
        /// <param name="input"></param>
        [Test]
        [TestCase("nestedcommand 2 subcommand 2")]
        [TestCase("nestedcommand 2 ")]
        [TestCase("nestedcommand subcommand")]
        [TestCase("subcommand 2 3 nestedcommand")]
        [TestCase("subcommand nestedcommand 2 2")]
        public void InvalidNestedSubcommandInputRaisesException(string input)
        {
            var splittedInput = input.Split(" ");

            toplevelParser.AddSubparser(subcommandParser);
            subcommandParser.AddSubparser(nestedsubcommandParser);

            Assert.That(() => toplevelParser.Parse(splittedInput),
                Throws.InstanceOf<ParserRuntimeException>());
        }

        /// <summary>
        /// Parser with two levels of subparsers.
        /// To the first level subparser we also add one additional flag to test
        /// founding of nestedcommand on more test cases.
        /// </summary>
        /// <param name="input"></param>
        [Test]
        [TestCase("subcommand 2 nestedcommand")]
        [TestCase("subcommand nestedcommand 2")]
        public void NestedCommandInputParsedSuccesfully(string input)
        {
            var splittedInput = input.Split(" ");

            toplevelParser.AddSubparser(subcommandParser);
            subcommandParser.AddSubparser(nestedsubcommandParser);

            toplevelParser.Parse(splittedInput);

            Assert.That(nestedsubcommandParser.Config.number, Is.EqualTo(2));
        }
    }
}
