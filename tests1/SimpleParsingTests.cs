namespace ArgparseTests;

/// <summary>
/// Tests for different parsing scenarios based on simple config 
/// </summary>
public class SimpleParsingTests
{
    private class TestConfig
    {
        public bool Flag { get; set; }
        public int IntValue { get; set; }
        public string? StringValue { get; set; }

        public List<int> IntList { get; set; } = new();
    }

    private TestConfig _config;
    private Parser<TestConfig> _parser;

    [SetUp]
    public void Setup()
    {
        _config = new TestConfig();
        _parser = new Parser<TestConfig>(_config)
        {
            Names = new[] { "program" },
            Description = "Program description"
        };

        var exampleFlag = new Flag<TestConfig>()
        {
            Names = new[] { "-f", "--flag" },
            Description = "Example flag",
            Action = storage => { storage.Flag = true; }
        };
        _parser.AddFlag(exampleFlag);

        var stringOption = new Option<TestConfig, string>
        {
            Names = new[]
            {
                "-s",
                "--string"
            },
            Description = "Example string option",
            Action = (storage, value) =>
            {
                storage.StringValue = value;
            },
            Converter = ConverterFactory.CreateStringConverter()
        };
        _parser.AddOption(stringOption);

        var intOption = new Option<TestConfig, int>
        {
            Names = new[]
            {
                "-i",
                "--int"
            },
            Description = "Example int option",
            Action = (storage, value) =>
            {
                storage.IntValue = value;
            },
            Converter = ConverterFactory.CreateIntConverter()
        };

        _parser.AddOption(intOption);

        var intListOption = new Option<TestConfig, List<int>>
        {
            Names = new[]
            {
                "-l",
                "--int-list"
            },
            Description = "Example int list option",
            Action = (storage, value) =>
            {
                storage.IntList = value;
            },
            Converter = ConverterFactory.CreateListConverter(ConverterFactory.CreateIntConverter())
        };

        _parser.AddOption(intListOption);
    }

    [Test]
    public void Parse_SetsFlag()
    {
        string[] args = { "--flag" };
        _parser.Parse(args);
        Assert.That(_config.Flag, Is.True);
    }

    [Test]
    public void Parse_SetsString()
    {
        string[] args = { "--string", "foo" };
        _parser.Parse(args);
        Assert.That(_config.StringValue, Is.EqualTo("foo"));
    }

    [Test]
    public void Parse_SetsStringMultipleWords()
    {
        string[] args = { "--string", "foo bar" };
        _parser.Parse(args);
        Assert.That(_config.StringValue, Is.EqualTo("foo bar"));
    }


    [Test]
    public void Parse_SetsInt()
    {
        string[] args = { "--int", "42" };
        _parser.Parse(args);
        Assert.That(_config.IntValue, Is.EqualTo(42));
    }

    [Test]
    public void Parse_SetsMultiple()
    {
        string[] args = { "--flag", "--string", "foo", "--int", "42" };

        _parser.Parse(args);

        Assert.IsTrue(_config.Flag);
        Assert.That(_config.StringValue, Is.EqualTo("foo"));
        Assert.That(_config.IntValue, Is.EqualTo(42));
    }

    [Test]
    public void Parse_DoesNotSetValues()
    {
        var args = Array.Empty<string>();

        _parser.Parse(args);

        Assert.IsFalse(_config.Flag);
        Assert.IsNull(_config.StringValue);
        Assert.That(_config.IntValue, Is.EqualTo(0));
        Assert.That(_config.IntList, Is.Empty);
    }

    [Test]
    public void Parse_SetsIntList()
    {
        string[] args = { "--int-list", "1", "2", "3" };
        _parser.Parse(args);
        Assert.That(_config.IntList, Is.EqualTo(new List<int> { 1, 2, 3 }));
    }

    [Test]
    public void Parse_SetsIntListAndInt()
    {
        string[] args = { "--int-list", "1", "2", "3", "--int", "42" };
        _parser.Parse(args);
        Assert.That(_config.IntList, Is.EqualTo(new List<int> { 1, 2, 3 }));
        Assert.That(_config.IntValue, Is.EqualTo(42));
    }

    [Test]
    public void Parse_PrintsHelp()
    {
        StringWriter stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        _parser.PrintHelp();

        var output = stringWriter.ToString();

        // TODO be more specific - I don't want to dictate the exact output format based on the implementation

        Assert.That(output, Does.Contain("program"));
        Assert.That(output, Does.Contain("Program description"));
        Assert.That(output, Does.Contain("-f, --flag"));
        Assert.That(output, Does.Contain("Example flag"));
        Assert.That(output, Does.Contain("-s, --string"));
        Assert.That(output, Does.Contain("Example string option"));
        Assert.That(output, Does.Contain("-i, --int"));
        Assert.That(output, Does.Contain("Example int option"));
        Assert.That(output, Does.Contain("-l, --int-list"));
        Assert.That(output, Does.Contain("Example int list option"));

        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()));
    }

    [Test]
    public void Parse_ThrowsOnMissingValue()
    {
        string[] args = { "--string" };
        Assert.Throws<ParserRuntimeException>(() => _parser.Parse(args));
    }

    [Test]
    public void Parse_ThrowsOnUnknownArgument()
    {
        string[] args = { "--unknown" };
        Assert.Throws<ParserRuntimeException>(() => _parser.Parse(args));
    }

    [Test]
    public void Parse_ThrowsOnDuplicateOption()
    {
        string[] args = { "--int", "42", "--int", "43" };
        Assert.Throws<ParserRuntimeException>(() => _parser.Parse(args));
    }

    [Test]
    public void Parse_ThrowsOnDuplicateFlag()
    {
        string[] args = { "--flag", "--flag" };
        Assert.Throws<ParserRuntimeException>(() => _parser.Parse(args));
    }

    [Test]
    public void Parse_ThrowsOnDuplicateFlagOneShort()
    {
        string[] args = { "--flag", "-f" };
        Assert.Throws<ParserRuntimeException>(() => _parser.Parse(args));
    }
    [Test]
    public void Parse_ThrowsOnInvalidInt()
    {
        string[] args = { "--int", "foo" };
        Assert.Throws<ParserConversionException>(() => _parser.Parse(args));
    }

    [Test]
    public void Parse_ThrowsOnInvalidIntList()
    {
        string[] args = { "--int-list", "1", "foo", "3" };
        Assert.Throws<ParserRuntimeException>(() => _parser.Parse(args));
    }
}