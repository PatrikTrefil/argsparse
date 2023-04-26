using Argparse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsArgparseAPI.ConfigTests
{
    public class ConfigFlagTests
    {

        [Test]
        public void FlagDefinitionAddedSuccesfully()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            parser.AddFlag(new Flag<TestConfiguration>
            {
                Names = new[] { "-h", "--help" },
                Description = "Show help",
                Action = storage => { storage.Help = true; }
            });

            var flags = parser.Flags;

            Assert.That(flags, Has.Count.EqualTo(1));
        }

        [Test]
        public void TwoFlagDefinitionsAddedSuccesfully()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            var helpFlag = new Flag<TestConfiguration>
            {
                Names = new[] { "-h", "--help" },
                Description = "Show help",
                Action = storage => { storage.Help = true; }
            };

            var versionFlag = new Flag<TestConfiguration>
            {
                Names = new[] { "-v", "--version" },
                Description = "Show version",
                Action = storage => { storage.Version = true; }
            };

            parser.AddFlags(new Flag<TestConfiguration>[] { helpFlag, versionFlag });

            var flags = parser.Flags;

            Assert.That(flags, Has.Count.EqualTo(2));
        }

        [Test]
        public void AddSameFlagInstantionTwiceFailsWithException()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            var helpFlag = new Flag<TestConfiguration>
            {
                Names = new[] { "-h", "--help" },
                Description = "Show help",
                Action = storage => { storage.Help = true; }
            };

            parser.AddFlag(helpFlag);

            Assert.That(() => parser.AddFlag(helpFlag),
                Throws.TypeOf<InvalidParserConfigurationException>());
        }

        [Test]
        public void AddFlagWithExistingNameFailsWithException()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            var helpFlag = new Flag<TestConfiguration>
            {
                Names = new[] { "-h", "--help" },
                Description = "Show help",
                Action = storage => { storage.Help = true; }
            };

            var helpFlag2 = new Flag<TestConfiguration>
            {
                Names = new[] { "--help" },
                Description = "Show second help",
                Action = storage => { storage.Help = true; }
            };

            parser.AddFlag(helpFlag);
            Assert.That(() => parser.AddFlag(helpFlag2),
                Throws.TypeOf<InvalidParserConfigurationException>());

        }

        [Test]
        public void AddFlagWithoutNameFailsWithException()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            Assert.That(() => new Flag<TestConfiguration>
            {
                Names = new string[] { },
                Description = "Show help",
                Action = storage => { storage.Help = true; }
            }, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void AddFlagWithInvalidNameFailsWithException()
        {
            var config = new TestConfiguration();
            var parser = new Parser<TestConfiguration>(config)
            {
                Names = new string[] { "test" },
                Description = "Test description."
            };

            Assert.That(() => new Flag<TestConfiguration>
            {
                Names = new string[] { "--valid", "invalid1", "invalid2" },
                Description = "Show help",
                Action = storage => { storage.Help = true; }
            }, Throws.TypeOf<ArgumentException>());
        }
    }
}
