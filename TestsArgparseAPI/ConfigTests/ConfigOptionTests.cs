using Argparse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsArgparseAPI.ConfigTests
{
    public class ConfigOptionTests
    {

        [Test]
        public void IntOptionDefinitionAddedSuccesfully()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            parser.AddOption(new Option<TestConfiguration, int>
            {
                Names = new[] { "-i", "--int" },
                Description = "Test option accepting int value",
                Action = (storage, value) => { storage.IntOption = value; },
                Converter = ConverterFactory.CreateIntConverter(),
            });

            var options = parser.Options;

            Assert.That(options, Has.Count.EqualTo(1));
        }

        [Test]
        public void StringOptionDefinitionAddedSuccesfully()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            parser.AddOption(new Option<TestConfiguration, string>
            {
                Names = new[] { "-s", "--str" },
                Description = "Test option accepting string value",
                Action = (storage, value) => { storage.StringOption = value; },
                Converter = ConverterFactory.CreateStringConverter(),
            });

            var options = parser.Options;

            Assert.That(options, Has.Count.EqualTo(1));
        }

        [Test]
        public void CustomTypeOptionDefinitionAddedSuccesfully()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            parser.AddOption(new Option<TestConfiguration, Person>
            {
                Names = new[] { "-p", "--person" },
                Description = "Test option accepting person value",
                Action = (storage, value) => { storage.PersonOption = value; },
                Converter = TestConfiguration.PersonConvertor,
            });

            var options = parser.Options;

            Assert.That(options, Has.Count.EqualTo(1));
        }

        [Test]
        public void TwoOptionDefinitionsAddedSuccesfully()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            var intOption = new Option<TestConfiguration, int>
            {
                Names = new[] { "-i", "--int" },
                Description = "Test option accepting int value",
                Action = (storage, value) => { storage.IntOption = value; },
                Converter = ConverterFactory.CreateIntConverter(),
            };

            var stringOption = new Option<TestConfiguration, string>
            {
                Names = new[] { "-s", "--str" },
                Description = "Test option accepting string value",
                Action = (storage, value) => { storage.StringOption = value; },
                Converter = ConverterFactory.CreateStringConverter(),
            };

            parser.AddOptions(new IOption<TestConfiguration>[] { intOption, stringOption });

            var options = parser.Options;

            Assert.That(options, Has.Count.EqualTo(2));
        }

        [Test]
        public void AddOptionInstantionTwiceFailsWithException()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            var stringOption = new Option<TestConfiguration, string>
            {
                Names = new[] { "-s", "--str" },
                Description = "Test option accepting string value",
                Action = (storage, value) => { storage.StringOption = value; },
                Converter = ConverterFactory.CreateStringConverter(),
            };

            parser.AddOption(stringOption);

            Assert.That(() => parser.AddOption(stringOption),
                Throws.TypeOf<InvalidParserConfigurationException>());
        }

        [Test]
        public void AddOptionWithExistingNameFailsWithException()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            var stringOption = new Option<TestConfiguration, string>
            {
                Names = new[] { "-s", "--str" },
                Description = "Test option accepting string value",
                Action = (storage, value) => { storage.StringOption = value; },
                Converter = ConverterFactory.CreateStringConverter(),
            };

            var stringOption2 = new Option<TestConfiguration, string>
            {
                Names = new[] { "-s" },
                Description = "Test option accepting string value",
                Action = (storage, value) => { storage.StringOption = value; },
                Converter = ConverterFactory.CreateStringConverter(),
            };

            parser.AddOption(stringOption);

            Assert.That(() => parser.AddOption(stringOption2),
                Throws.TypeOf<InvalidParserConfigurationException>());
        }
    }
}
