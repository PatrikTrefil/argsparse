using Argparse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TestsArgparseAPI.ParsingTests
{
    public record ArgumentTestConfig
    {
        public string StringArgument = "";
        public int IntArgument = 0;
        public List<int> MultipleIntArgument = new();
        public List<int> ThreeTimesIntArgument = new();

    }

    internal class ArgumentParsingTests
    {
        private ArgumentTestConfig config;
        private Parser<ArgumentTestConfig> parser;

        [SetUp]
        public void ArgumentTestSetup()
        {
            config = new ArgumentTestConfig();
            parser = new Parser<ArgumentTestConfig>(config)
            {
                Names = new string[] { "argumentTest" },
                Description = "test inputs for arguments"
            };

            parser.AddArgument(new Argument<ArgumentTestConfig, int>
            {
                ValuePlaceholder = "intArg",
                Description = "Integer argument",
                Action = (storage, value) => { storage.IntArgument = value; },
                Multiplicity = new ArgumentMultiplicity.SpecificCount(1, true),
                Converter = ConverterFactory.CreateIntConverter()
            });

            parser.AddArgument(new Argument<ArgumentTestConfig, string>
            {
                ValuePlaceholder = "stringArg",
                Description = "String argument",
                Action = (storage, value) => { storage.StringArgument = value; },
                Multiplicity = new ArgumentMultiplicity.SpecificCount(1, true),
                Converter = ConverterFactory.CreateStringConverter()
            });


            parser.AddArgument(new Argument<ArgumentTestConfig, int>
            {
                ValuePlaceholder = "intArg3",
                Description = "Integer argument repeated 3 times",
                Action = (storage, value) => { storage.ThreeTimesIntArgument.Add(value); },
                Multiplicity = new ArgumentMultiplicity.SpecificCount(3, true),
                Converter = ConverterFactory.CreateIntConverter()
            });


            parser.AddArgument(new Argument<ArgumentTestConfig, int>
            {
                ValuePlaceholder = "multipleIntArg",
                Description = "Integer argument that takes all arguments left",
                Action = (storage, value) => { storage.MultipleIntArgument.Add(value); },
                Multiplicity = new ArgumentMultiplicity.AllThatFollow(),
                Converter = ConverterFactory.CreateIntConverter()
            });
        }

        [Test]
        [TestCase("1", "text", "1 2 3")]
        [TestCase("1", "textik", "1 2 4")]
        public void ParseStringArgument(string intArg, string stringArg, string multipleInt)
        {
            var input = (intArg + ' ' + stringArg + ' ' + multipleInt).Split(' ');
            parser.Parse(input);

            Assert.That(config.StringArgument, Is.EqualTo(stringArg));
        }

        [Test]
        [TestCase(1, "text")]
        [TestCase(2, "text")]
        [TestCase(-4, "text")]
        public void ParseIntArgument(int intArg, string stringArg)
        {
            var input = (intArg.ToString() + ' ' + stringArg + ' ' + "1 2 3").Split(' ');
            parser.Parse(input);

            Assert.That(config.IntArgument, Is.EqualTo(intArg));
        }

        [Test]
        [TestCase("1",  "text", "1 2 3", "1")]
        public void ParseIntArgumentWithFixedNumberOfOccurrences(string intArg,  string stringArg, string fixedNumberArg, string multipleInt)
        {
            var input = (intArg.ToString() + ' '  + stringArg + ' ' + fixedNumberArg + ' ' + multipleInt).Split(' ');
            parser.Parse(input);

            Assert.Multiple(() =>
            {
                Assert.That(config.ThreeTimesIntArgument.Count, Is.EqualTo(3));
                Assert.That(config.ThreeTimesIntArgument[0], Is.EqualTo(1));
                Assert.That(config.ThreeTimesIntArgument[1], Is.EqualTo(2));
                Assert.That(config.ThreeTimesIntArgument[2], Is.EqualTo(3));
            });
        }

        /// <summary>
        /// Correct parsing of AllThatFollowArgument is achieved by
        /// - checking that the list in config contains the same number of values as the input
        /// - checking that the list in config contains one concrete value from input
        /// </summary>
        /// <param name="stringArg"></param>
        /// <param name="intArg"></param>
        /// <param name="multipleInt"></param>
        [Test]
        [TestCase("1", "text", "1 2 3", "1 2 3 4 5 -5 6 -7")]
        [TestCase("1", "text", "1 2 3", "1 2 3 4 5")]
        [TestCase("1", "text", "1 2 3", "5 4 6")]
        public void ParseAllThatFollowArgument(string intArg, string stringArg, string ints, string multipleInt)
        {
            var input = (intArg + ' ' + stringArg + ' ' + ints + ' ' + multipleInt).Split(' ');
            parser.Parse(input);
            var list = multipleInt.Split(" ");

            Assert.Multiple(() =>
            {
                Assert.That(config.MultipleIntArgument.Count, Is.EqualTo(list.Count()));
                Assert.That(config.MultipleIntArgument.Contains(5));
            });
        }

        [Test]
        [TestCase("1 text 1 2 3")]
        [TestCase("1 text 1 2 3")]
        public void ParseEmptyAllThatFollowArgument(string testCase)
        {
            var input = testCase.Split(' ');
            parser.Parse(input);

            Assert.That(config.MultipleIntArgument.Count, Is.EqualTo(0));
        }

        [Test]
        [TestCase("1", "text", "1 2 3", "1")]
        public void ParseNotRequiredArgumentThatIsPresent(string intArg,  string stringArg, string fixedNumberArg, string multipleInt)
        {
            var input = (intArg.ToString() + ' ' + stringArg + ' ' + fixedNumberArg + ' ' + multipleInt).Split(' ');
            parser.Parse(input);


            Assert.That(config.ThreeTimesIntArgument.Count, Is.EqualTo(3));
        }


        [Test]
        [TestCase("", "", "")]
        public void MissingRequiredArgumentRaisesException(string intArg, string stringArg, string multipleInt)
        {
            var input = (stringArg + ' ' + intArg + ' ' + multipleInt).Split(' ');

            Assert.That(() => parser.Parse(input),
                Throws.TypeOf<ParserConversionException>());
        }

        [Test]
        [TestCase("text text 1 2 3 1 2")]
        [TestCase("1 text text text text 1 2")]
        [TestCase("1 text 1 2 text text 1 2")]
        [TestCase("1 text 1 2 text 1 2")]
        [TestCase("1 text 1 2 3 1 2 text")]
        [TestCase("1 text 1 2 3 text")]
        [TestCase("1 text 1 2 text")]
        [TestCase("1 text text 1 2")]
        [TestCase("1 text 1 2 3 1 2 1,5")]
        [TestCase("1,0 text 1 2 3 1 2 text")]
        public void WrongInputRaisesConversionException(string testCase)
        {
            var input = testCase.Split(' ');

            Assert.That(() => parser.Parse(input),
                Throws.TypeOf<ParserConversionException>());
        }


    }
}
