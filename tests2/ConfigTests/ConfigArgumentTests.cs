using Argparse;

namespace TestsArgparseAPI.ConfigTests
{
    public class ConfigArgumentTests
    {

        [Test]
        public void IntArgumentDefinitionAddedSuccesfully()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            parser.AddArgument(new Argument<TestConfiguration, int>
            {
                Description = "Integer argument",
                Action = (storage, value) => { storage.IntArgument = value; },
                Converter = ConverterFactory.CreateIntConverter(),
            });

            var arguments = parser.Arguments;

            Assert.That(arguments, Has.Count.EqualTo(1));
        }

        [Test]
        public void StringArgumentDefinitionAddedSuccesfully()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            parser.AddArgument(new Argument<TestConfiguration, string>
            {
                Description = "String argument",
                Action = (storage, value) => { storage.StringArgument = value; },
                Converter = ConverterFactory.CreateStringConverter(),
            });

            var arguments = parser.Arguments;

            Assert.That(arguments, Has.Count.EqualTo(1));
        }

        [Test]
        public void CustomTypeArgumentDefinitionAddedSuccesfully()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            parser.AddArgument(new Argument<TestConfiguration, Person>
            {
                Description = "Person argument",
                Action = (storage, value) => { storage.PersonArgument = value; },
                Converter = TestConfiguration.PersonConvertor,
            });

            var arguments = parser.Arguments;

            Assert.That(arguments, Has.Count.EqualTo(1));
        }

        [Test]
        public void TwoArgumentDefinitionsAddedSuccesfully()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            var intArgument = new Argument<TestConfiguration, int>
            {
                Description = "Integer argument",
                Action = (storage, value) => { storage.IntArgument = value; },
                Converter = ConverterFactory.CreateIntConverter(),
            };

            var stringArgument = new Argument<TestConfiguration, string>
            {
                Description = "String argument",
                Action = (storage, value) => { storage.StringArgument = value; },
                Converter = ConverterFactory.CreateStringConverter(),
            };

            parser.AddArguments(new IArgument<TestConfiguration>[] { intArgument, stringArgument });

            var arguments = parser.Arguments;

            Assert.That(arguments, Has.Count.EqualTo(2));
        }

        [Test]
        public void AddArgumentInstantionTwiceFailsWithException()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            var stringArgument = new Argument<TestConfiguration, string>
            {
                Description = "String argument",
                Action = (storage, value) => { storage.StringArgument = value; },
                Converter = ConverterFactory.CreateStringConverter(),
            };

            parser.AddArgument(stringArgument);

            Assert.That(() => parser.AddArgument(stringArgument),
                Throws.TypeOf<InvalidParserConfigurationException>());
        }

        /// <summary>
        /// To specify, that the argument is not required, it is needed to set
        /// isRequired parameter in multiplicity of the argument to false.
        /// e.g. ArgumentMultiplicity.SpecificCount(n, false)
        /// </summary>
        [Test]
        public void AddTwoNotRequiredArgumentsToParserReaisesException()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            var intArgument = new Argument<TestConfiguration, int>
            {
                Description = "Integer argument",
                Action = (storage, value) => { storage.IntArgument = value; },
                Converter = ConverterFactory.CreateIntConverter(),
                Multiplicity = new ArgumentMultiplicity.SpecificCount(1, false),
            };

            var stringArgument = new Argument<TestConfiguration, string>
            {
                Description = "String argument",
                Action = (storage, value) => { storage.StringArgument = value; },
                Converter = ConverterFactory.CreateStringConverter(),
                Multiplicity = new ArgumentMultiplicity.SpecificCount(1, false),
            };

            parser.AddArgument(intArgument);

            Assert.That(() => parser.AddArgument(stringArgument),
                Throws.TypeOf<InvalidParserConfigurationException>());
        }

        [Test]
        public void AddTwoArgumentsWithAllThatFollowMultiplicityToParserReaisesException()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            var intAllThatFollowArgument = new Argument<TestConfiguration, int>
            {
                Description = "Integer argument",
                Action = (storage, value) => { storage.IntArgument = value; },
                Converter = ConverterFactory.CreateIntConverter(),
                Multiplicity = new ArgumentMultiplicity.AllThatFollow(),
            };

            var stringAllThatFollowArgument = new Argument<TestConfiguration, string>
            {
                Description = "String argument",
                Action = (storage, value) => { storage.StringArgument = value; },
                Converter = ConverterFactory.CreateStringConverter(),
                Multiplicity = new ArgumentMultiplicity.AllThatFollow(),
            };

            parser.AddArgument(intAllThatFollowArgument);

            Assert.That(() => parser.AddArgument(stringAllThatFollowArgument),
                Throws.TypeOf<InvalidParserConfigurationException>());
        }

        /// <summary>
        /// The assumption was, that parser is trying to parse arguments in the input
        /// in the same order their definition were added to parser.
        /// Therefore it makes no sense to add argument with fixed number of items
        /// after the argument of AllThatFollow type.
        /// </summary>
        [Test]
        public void AddRequiredArgumentAfterAllThatFollowArgumentToParserReaisesException()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            var intAllThatFollowArgument = new Argument<TestConfiguration, int>
            {
                Description = "Integer argument",
                Action = (storage, value) => { storage.IntArgument = value; },
                Converter = ConverterFactory.CreateIntConverter(),
                Multiplicity = new ArgumentMultiplicity.AllThatFollow(),
            };

            var stringRequiredArgument = new Argument<TestConfiguration, string>
            {
                Description = "String argument",
                Action = (storage, value) => { storage.StringArgument = value; },
                Converter = ConverterFactory.CreateStringConverter(),
                Multiplicity = new ArgumentMultiplicity.SpecificCount(1, true),
            };

            parser.AddArgument(intAllThatFollowArgument);

            Assert.That(() => parser.AddArgument(stringRequiredArgument),
                Throws.TypeOf<InvalidParserConfigurationException>());
        }

        [Test]
        public void AddNotRequiredArgumentAfterAllThatFollowArgumentToParserReaisesException()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            var intAllThatFollowArgument = new Argument<TestConfiguration, int>
            {
                Description = "Integer argument",
                Action = (storage, value) => { storage.IntArgument = value; },
                Converter = ConverterFactory.CreateIntConverter(),
                Multiplicity = new ArgumentMultiplicity.AllThatFollow(),
            };

            var stringNotRequiredArgument = new Argument<TestConfiguration, string>
            {
                Description = "String argument",
                Action = (storage, value) => { storage.StringArgument = value; },
                Converter = ConverterFactory.CreateStringConverter(),
                Multiplicity = new ArgumentMultiplicity.SpecificCount(1, false),
            };

            parser.AddArgument(intAllThatFollowArgument);

            Assert.That(() => parser.AddArgument(stringNotRequiredArgument),
                Throws.TypeOf<InvalidParserConfigurationException>());
        }

        [Test]
        public void AddArgumentAfterArgumentWithMultiplicityAllThatFollowsRaisesException()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            var intAllThatFollowArgument = new Argument<TestConfiguration, int>
            {
                Description = "Integer argument",
                Action = (storage, value) => { storage.IntArgument = value; },
                Converter = ConverterFactory.CreateIntConverter(),
                Multiplicity = new ArgumentMultiplicity.AllThatFollow(),
            };

            var stringArgument = new Argument<TestConfiguration, string>
            {
                Description = "String argument",
                Action = (storage, value) => { storage.StringArgument = value; },
                Converter = ConverterFactory.CreateStringConverter(),
            };

            parser.AddArgument(intAllThatFollowArgument);

            Assert.That(() => parser.AddArgument(stringArgument),
                Throws.TypeOf<InvalidParserConfigurationException>());
        }
    }
}
