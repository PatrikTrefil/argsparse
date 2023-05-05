using Argparse;

namespace TestsArgparseAPI.ParsingTests
{


    public record MixedConfig
    {
        public class HelpPassed : Exception { }
        public int IntOption;
        public string RequiredStringOption;
        public bool BoolOption;
        public List<int> ListOption = new();
        public List<string> AllThatFollowsArgument = new();
        public int NotRequiredArgument;
        public string RequiredArgument;
    }

    /// <summary>
    /// Tests provide scenarios where all types of input arguments are mixed
    /// together in the input.
    /// All definitions for flags, options and arguments are added to parser in
    /// MixedTestSetup.
    /// Two parametric tests are testing parser on invalid inputs.
    /// The last test is testing expected --help flag behaviour.
    /// </summary>
    internal class MixedParsingTests
    {
        public MixedConfig config;
        public Parser<MixedConfig> parser;
        [SetUp]
        public void MixedTestsSetup()
        {
            config = new MixedConfig();
            parser = new Parser<MixedConfig>(config)
            {
                Names = new() { "mixed test" },
                Description = "test mixed inputs"
            };

            parser.AddFlag(new Flag<MixedConfig>
            {
                Names = new() { "--help", "-h" },
                Description = "prints help message",
                Action = (storage) => { throw new MixedConfig.HelpPassed(); },
            });


            parser.AddOption(new Option<MixedConfig, string>
            {
                Names = new() { "-s", "--required-string" },
                Description = "option accepting required string value",
                Action = (storage, value) => { storage.RequiredStringOption = value; },
                IsRequired = true,
                Converter = ConverterFactory.CreateStringConverter(),
            });

            parser.AddOption(new Option<MixedConfig, bool>
            {
                Names = new() { "-b", "--bool" },
                Description = "option accepting bool value",
                Action = (storage, value) => { storage.BoolOption = value; },
                Converter = ConverterFactory.CreateBoolConverter(),
            });

            parser.AddOption(new Option<MixedConfig, int>
            {
                Names = new() { "-i", "--int" },
                Description = "option accepting int value",
                Action = (storage, value) => { storage.IntOption = value; },
                Converter = ConverterFactory.CreateIntConverter(),
            });

            parser.AddOption(new Option<MixedConfig, List<int>>
            {
                Names = new() { "-l", "--list" },
                Description = "option accepting list of int value",
                Action = (storage, value) => { storage.ListOption = value; },
                Converter = ConverterFactory.CreateListConverter<int>(ConverterFactory.CreateIntConverter(), ',')
            });

            parser
                .AddArgument(new Argument<MixedConfig, string>
                {
                    Description = "required argument",
                    Converter = ConverterFactory.CreateStringConverter(),
                    Action = (storage, value) => { storage.RequiredArgument = value; },
                })
                .AddArgument(new Argument<MixedConfig, int>
                {
                    Description = "not required argument",
                    Converter = ConverterFactory.CreateIntConverter(),
                    Action = (storage, value) => { storage.NotRequiredArgument = value; },
                    Multiplicity = new ArgumentMultiplicity.SpecificCount(1, false),
                });
        }

        [Test]
        [TestCase("-s=true -i true -- value")]
        [TestCase("-s=true -b 3 -- value")]
        [TestCase("-b value -s=true -- value")]
        [TestCase("-l true -s=true -i true -- value")]
        [TestCase("-l=1,2,3,4 -i value -s=true -- value")]
        [TestCase("-s value -i 2 - value")]
        [TestCase("- s value -- value")]
        [TestCase("-s value -i 2 - value")]
        public void InvalidInputRaisesConversionException(string testCase)
        {
            string[] input = testCase.Split(" ");

            Assert.That(() => parser.Parse(input),
                Throws.TypeOf<ParserConversionException>());
        }

        [Test]
        [TestCase("-- -i 2 -s value -- value")]
        [TestCase("-i 2 -s=value --")]
        [TestCase("-s value -i 2")]
        [TestCase("-i 2 -- value")]
        [TestCase("-- value 23 k l m 5")]
        [TestCase("-b false --int=3 -s value")]
        public void InvalidInputRaisesRuntimeException(string testCase)
        {
            string[] input = testCase.Split(" ");

            Assert.That(() => parser.Parse(input),
                Throws.TypeOf<ParserRuntimeException>());
        }



        [Test]
        [TestCase("--help")]
        [TestCase("--help -s=value -- value")]
        [TestCase("--help -- value")]
        [TestCase("--help -b=true -- value")]
        [TestCase("--help -- value 23 4 5 6 4 5 8 7 true")]
        [TestCase("-h -- value 2")]
        public void NothingIsParsedAfterHelpFlag(string testCase)
        {
            string[] input = testCase.Split(" ");


            Assert.That(() => parser.Parse(input),
              Throws.TypeOf<MixedConfig.HelpPassed>());
            Assert.That(config.RequiredArgument, Is.Null);
        }
    }
}
