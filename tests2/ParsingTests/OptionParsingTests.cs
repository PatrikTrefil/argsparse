using Argparse;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestsArgparseAPI.ConfigTests;

namespace TestsArgparseAPI.ParsingTests
{
    /// <summary>
    /// Option record consists options of almost
    /// all types with predefined convertors.
    /// There is also one option of a custom type Person, 
    /// the definition can be found in ConfigTests.ConfigTestsHelpClasses.
    /// </summary>
    public record OptionTestsConfig
    {
        public int IntOption = 0;
        public string StringOption = "";
        public bool BoolOption = false;
        public Person PersonOption = new();
        public List<int> ListOption = new();

        public static Person PersonConvertor(string value)
        {
            var parsed = value.Split(',');
            return new Person { Name = parsed[0], Address = parsed[1] };
        }
    }

    public record RequiredOptionTestsConfig
    {
        public int IntRequiredOption = 0;
        public string StringOption = "";
        public string SecondStringOption = "";
    }

    /// <summary>
    /// Tests are working with two parsers - first for testing
    /// the behaviour with required option, second for all other
    /// use cases. Such decision was made to simplify test entries
    /// for the use cases where the required option is not needed.
    /// </summary>
    public class OptionParsingTests
    {
        private OptionTestsConfig config;
        private Parser<OptionTestsConfig> parser;
        private RequiredOptionTestsConfig requiredOptionConfig;
        private Parser<RequiredOptionTestsConfig> requiredOptionParser;

        [SetUp]
        public void RequiredOptionTestsSetup()
        {
            requiredOptionConfig = new RequiredOptionTestsConfig();
            requiredOptionParser = new Parser<RequiredOptionTestsConfig>(requiredOptionConfig)
            {
                Names = new string[] { "requiredOptionTest" },
                Description = "test inputs for required option"
            };

            requiredOptionParser.AddOption(new Option<RequiredOptionTestsConfig, string>
            {
                Names = new[] { "-s", "--str" },
                Description = "Test option accepting string value",
                Action = (storage, value) => { storage.StringOption = value; },
                Converter = ConverterFactory.CreateStringConverter(),
            });

            requiredOptionParser.AddOption(new Option<RequiredOptionTestsConfig, string>
            {
                Names = new[] { "-S", "--Sstr" },
                Description = "Test option accepting string value",
                Action = (storage, value) => { storage.SecondStringOption = value; },
                Converter = ConverterFactory.CreateStringConverter(),
            });

            requiredOptionParser.AddOption(new Option<RequiredOptionTestsConfig, int>
            {
                Names = new[] { "-i", "--int" },
                Description = "Test required option accepting int value",
                Action = (storage, value) => { storage.IntRequiredOption = value; },
                IsRequired = true,
                Converter = ConverterFactory.CreateIntConverter(),
            });
        }

        [SetUp]
        public void OptionTestsSetup()
        {
            config = new OptionTestsConfig();
            parser = new Parser<OptionTestsConfig>(config)
            {
                Names = new string[] { "optionTest" },
                Description = "test inputs for options"
            };

            parser.AddOption(new Option<OptionTestsConfig, string>
            {
                Names = new[] { "-s", "--str" },
                Description = "Test option accepting string value",
                Action = (storage, value) => { storage.StringOption = value; },
                Converter = ConverterFactory.CreateStringConverter(),
            });

            parser.AddOption(new Option<OptionTestsConfig, int>
            {
                Names = new[] { "-i", "--int" },
                Description = "Test option accepting int value",
                Action = (storage, value) => { storage.IntOption = value; },
                Converter = ConverterFactory.CreateIntConverter(),
            });

            parser.AddOption(new Option<OptionTestsConfig, bool>
            {
                Names = new[] { "-b", "--bool" },
                Description = "Test option accepting bool value",
                Action = (storage, value) => { storage.BoolOption = value; },
                Converter = ConverterFactory.CreateBoolConverter(),
            });

            parser.AddOption(new Option<OptionTestsConfig, Person>
            {
                Names = new[] { "-p", "--person" },
                Description = "Test option accepting person value",
                Action = (storage, value) => { storage.PersonOption = value; },
                Converter = TestConfiguration.PersonConvertor,
            });

            parser.AddOption(new Option<OptionTestsConfig, List<int>>
            {
                Names = new[] { "-l", "--list" },
                Description = "Test option accepting list of int value.",
                Action = (storage, value) => { storage.ListOption = value; },
                Converter = ConverterFactory.CreateListConverter<int>(ConverterFactory.CreateIntConverter(), ',')
            });
        }

        /// <summary>
        /// In this test we simulate input in the command line by concatenating
        /// option + separator + value. Then we construct input for Parser
        /// by splitting concatenated string by ' '. As a result of splitting
        /// we obtain either one string containing '=' (on the left is option name, 
        /// on the rigth value), or two strings (option and value).
        /// 
        /// At the end we assert, that the parsed value is the same as in the input.
        /// </summary>
        /// <param name="option">Short or long name of option.</param>
        /// <param name="separator">Separator between option name and value, either ' ' or '='</param>
        /// <param name="value">String value assigned to option</param>
        [Test]
        [TestCase("--str", "=", "value")]
        [TestCase("-s", "=", "value")]
        [TestCase("--str", " ", "value")]
        [TestCase("-s", " ", "value")]
        [TestCase("-s", " ", "value")]
        public void ParseStringOptionWithValue(string option, string separator, string value)
        {
            var input = (option + separator + value).Split(' ');
            parser.Parse(input);

            Assert.That(config.StringOption, Is.EqualTo(value));
        }

        [Test]
        [TestCase("--int", "=", 2)]
        [TestCase("-i", "=", 0)]
        [TestCase("--int", " ", -10)]
        [TestCase("-i", " ", int.MaxValue)]
        [TestCase("-i", "=", int.MinValue)]
        public void ParseIntegerOptionWithValue(string option, string separator, int value)
        {
            var input = (option + separator + value.ToString()).Split(' ');
            parser.Parse(input);

            Assert.That(config.IntOption, Is.EqualTo(value));
        }

        [Test]
        [TestCase("--bool", "=", true)]
        [TestCase("-b", "=", false)]
        [TestCase("--bool", " ", false)]
        [TestCase("-b", " ", true)]
        public void ParseBoolOptionWithValue(string option, string separator, bool value)
        {
            var input = (option + separator + value.ToString()).Split(' ');
            parser.Parse(input);

            Assert.That(config.BoolOption, Is.EqualTo(value));
        }

        [Test]
        [TestCase("--person", "=", "Pepa,Modranska")]
        [TestCase("-p", "=", "Vasek,Malostranska")]
        [TestCase("--person", " ", "Maruska,Dlouha")]
        [TestCase("-p", " ", "Klara,Bojasova")]
        public void ParseCustomTypeOptionWithValue(string option, string separator, string value)
        {
            Person p = OptionTestsConfig.PersonConvertor(value);
            var input = (option + separator + value).Split(' ');
            parser.Parse(input);

            Assert.Multiple(() =>
            {
                Assert.That(config.PersonOption.Name, Is.EqualTo(p.Name));
                Assert.That(config.PersonOption.Address, Is.EqualTo(p.Address));
            });
        }

        [Test]
        [TestCase("--list", "=", new int[] { 1, 2, 3 })]
        [TestCase("-l", "=", new int[] { 1, 2, 3, 4, 5, 6, 7 })]
        [TestCase("--list", " ", new int[] { 1 })]
        [TestCase("-l", " ", new int[] { 0 })]
        public void ParseListOptionWithValue(string option, string separator, int[] value)
        {
            var list = value.ToList();
            var input = (option + separator + string.Join(",", value)).Split(' ');
            parser.Parse(input);

            Assert.That(config.ListOption, Is.EqualTo(list));
        }

        [Test]
        [TestCase("--list")]
        [TestCase("-s")]
        [TestCase("--int")]
        public void ParsingOptionWithoutValueRaisesException(string testCase)
        {
            string[] input = testCase.Split(" ");

            Assert.That(() => parser.Parse(input),
                Throws.TypeOf<ParserRuntimeException>());
        }

        [TestCase("-p --int")]
        [TestCase("-l -s")]
        public void ParsingOptionWithoutRaisesException(string testCase)
        {
            string[] input = testCase.Split(" ");

            Assert.That(() => parser.Parse(input),
                Throws.TypeOf<ParserConversionException>());
        }

        /// <summary>
        /// Tests situation, where two options were provided in the input.
        /// first option is IntegerOption, second is StringOption
        /// </summary>
        /// <param name="firstOption"></param>
        /// <param name="firstSeparator"></param>
        /// <param name="firstValue"></param>
        /// <param name="secondOption"></param>
        /// <param name="secondSeparator"></param>
        /// <param name="secondValue"></param>
        [Test]
        [TestCase("-i", "=", 0, "-s", "=", "first")]
        [TestCase("--int", "=", 1, "-s", "=", "second")]
        [TestCase("-i", "=", -2, "--str", " ", "third")]
        [TestCase("-i", " ", 3, "--str", "=", "fourth")]
        [TestCase("--int", " ", 4, "--str", " ", "fifth")]
        public void ParseTwoOptionsWithValues(
            string firstOption,
            string firstSeparator,
            int firstValue,
            string secondOption,
            string secondSeparator,
            string secondValue)
        {
            var input = (firstOption + firstSeparator + firstValue.ToString() + " " + secondOption + secondSeparator + secondValue).Split(' ');
            parser.Parse(input);

            Assert.Multiple(() =>
            {
                Assert.That(config.IntOption, Is.EqualTo(firstValue));
                Assert.That(config.StringOption, Is.EqualTo(secondValue));
            });
        }

        [Test]
        [TestCase("--str=")]
        [TestCase("-s= -i")]
        [TestCase("-i=")]
        [TestCase("-i")]
        [TestCase("--int= -l")]
        public void ParsingOptionWithEmptyValueRaisesException(string testCase)
        {
            var input = (testCase).Split(' ');

            Assert.That(() => parser.Parse(input),
                Throws.TypeOf<ParserRuntimeException>());
        }

        [Test]
        [TestCase("-l", "=", "wrong")]
        [TestCase("-i", " ", "wrong")]
        [TestCase("-p", "=", "wrong")]
        [TestCase("--list", "=", ",")]
        [TestCase("--list", " ", "2,3,-2,mm")]
        public void ParsingOptionWithWrongAssignedValueRaisesException(string option, string separator, string value)
        {
            var input = (option + separator + value).Split(' ');

            Assert.That(() => parser.Parse(input),
                Throws.TypeOf<ParserConversionException>());
        }

        [Test]
        [TestCase("-s=value")]
        [TestCase("--str value -S value")]
        [TestCase("-s=value -S value")]
        public void RequiredOptionNotProvidedRaisesException(string testCase)
        {
            var input = testCase.Split(' ');

            Assert.That(() => requiredOptionParser.Parse(input),
                Throws.TypeOf<ParserRuntimeException>());
        }

    }
}
