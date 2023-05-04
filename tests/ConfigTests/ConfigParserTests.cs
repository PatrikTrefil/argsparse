using Argparse;

namespace TestsArgparseAPI.ConfigTests
{
    public class ConfigParserTests
    {
        [Test]
        public void CreateParserInstanceWithoutExistingInstanceOfConfigRecord()
        {
            var parser = new Parser<TestConfiguration>(() => new TestConfiguration())
            {
                Names = new string[] { "test" },
                Description = "Test parser."
            };

            parser.Parse(new string[] { });

            Assert.IsNotNull(parser.Config);
        }

    }
}
