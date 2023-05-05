using Argparse;

namespace TestsArgparseAPI.ParsingTests
{
    public record FlagTestConfig
    {
        public bool Help = false;
        public bool Version = false;
    }

    public class FlagParsingTests
    {
        private FlagTestConfig config;
        private Parser<FlagTestConfig> parser;

        [SetUp]
        public void FlagTestsSetup()
        {
            config = new FlagTestConfig();
            parser = new Parser<FlagTestConfig>(config)
            {
                Names = new() { "flagTest" },
                Description = "test inputs for flags"
            };

            parser.AddFlag(new Flag<FlagTestConfig>
            {
                Names = new() { "-h", "--help" },
                Description = "Show help",
                Action = storage => { storage.Help = true; }
            });

            parser.AddFlag(new Flag<FlagTestConfig>
            {
                Names = new() { "-v", "--version" },
                Description = "Show version",
                Action = storage => { storage.Version = true; }
            });

        }

        [Test]
        [TestCase("-h")]
        [TestCase("-v")]
        public void ParseFlagAccesedViaShortName(string shortName)
        {
            parser.Parse(new string[] { shortName });

            switch (shortName)
            {
                case "-h":
                    Assert.That(config.Help, Is.True);
                    break;
                case "-v":
                    Assert.That(config.Version, Is.True);
                    break;
            }
        }

        [Test]
        [TestCase("--help")]
        [TestCase("--version")]
        public void ParseFlagAccesedViaLongName(string longName)
        {
            parser.Parse(new string[] { longName });

            switch (longName)
            {
                case "--help":
                    Assert.That(config.Help, Is.True);
                    break;
                case "--version":
                    Assert.That(config.Version, Is.True);
                    break;
            }
        }

        [Test]
        [TestCase("--help", "--version")]
        [TestCase("--version", "-h")]
        [TestCase("--help", "-v")]
        [TestCase("-v", "-h")]
        public void ParseMultipleFlags(string firstFlag, string secondFlag)
        {
            parser.Parse(new string[] { firstFlag, secondFlag });

            Assert.Multiple(() =>
            {
                Assert.That(config.Help, Is.True);
                Assert.That(config.Version, Is.True);
            });
        }

        [Test]
        public void FlagsAppearedMultipleTimesRaisesException()
        {
            string[] input = new string[] { "--help", "-h" };

            Assert.That(() => parser.Parse(input),
                Throws.TypeOf<ParserRuntimeException>());
        }

        [Test]
        [TestCase("-h=value")]
        [TestCase("--help=4")]
        [TestCase("--help 4")]
        public void FlagAssignedWithValueRaisesException(string testCase)
        {
            string[] input = testCase.Split(" ");

            Assert.That(() => parser.Parse(input),
                Throws.TypeOf<ParserRuntimeException>());
        }

    }
}
