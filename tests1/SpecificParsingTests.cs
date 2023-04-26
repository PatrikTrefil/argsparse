namespace ArgparseTests;

/// <summary>
/// Tests for different parsing scenarios based on specific configs
/// </summary>
public class SpecificParsingTests
{
    private class ArgumentConfig
    {
        public string? StringValue { get; set; }
        public int IntValue { get; set; }
        public List<int> IntList { get; set; } = new();
    }

    [Test]
    public void Parse_Arguments()
    {
        var config = new ArgumentConfig();
        var parser = new Parser<ArgumentConfig>(config)
        {
            Names = new[]
                {"program"},
            Description = null
        };

        var stringArgument = new Argument<ArgumentConfig, string>
        {
            Description = "Example string argument",
            ValuePlaceholder = "<string>",
            Converter = ConverterFactory.CreateStringConverter(),
            Action = (storage, value) =>
            {
                storage.StringValue = value;
            },
            Multiplicity = new ArgumentMultiplicity.SpecificCount(Number: 1, IsRequired: true)
        };
        parser.AddArgument(stringArgument);

        var intArgument = new Argument<ArgumentConfig, int>
        {
            Description = "Example int argument",
            ValuePlaceholder = "<int>",
            Converter = ConverterFactory.CreateIntConverter(),
            Action = (storage, value) =>
            {
                storage.IntValue = value;
            },
            Multiplicity = new ArgumentMultiplicity.SpecificCount(Number: 1, IsRequired: true)
        };
        parser.AddArgument(intArgument);

        var intListArgument = new Argument<ArgumentConfig, int>
        {
            Description = "Example int list argument",
            ValuePlaceholder = "<int>",
            Converter = ConverterFactory.CreateIntConverter(),
            Action = (storage, value) =>
            {
                storage.IntList.Add(value);
            },
            Multiplicity = new ArgumentMultiplicity.SpecificCount(Number: 2, IsRequired: false)
        };
        parser.AddArgument(intListArgument);

        string[] args = { "foo", "1", "2", "3" };
        parser.Parse(args);
        Assert.That(config.StringValue, Is.EqualTo("foo"));
        Assert.That(config.IntValue, Is.EqualTo(1));
        Assert.That(config.IntList, Is.EqualTo(new List<int> { 2, 3 }));
    }

    [Test]
    public void Parse_ArgumentAfterDoubleDash()
    {
        var config = new ArgumentConfig();
        var parser = new Parser<ArgumentConfig>(config)
        {
            Names = new[]
                {"program"},
            Description = null
        };

        var intOption = new Option<ArgumentConfig, int>
        {
            Names = new[] { "--int-option", "-i" },
            Description = "Example int option",
            Converter = ConverterFactory.CreateIntConverter(),
            Action = (storage, value) =>
            {
                storage.IntValue = value;
            }
        };

        parser.AddOption(intOption);

        var stringArgument = new Argument<ArgumentConfig, string>
        {
            Description = "Example string argument",
            ValuePlaceholder = "<string>",
            Converter = ConverterFactory.CreateStringConverter(),
            Action = (storage, value) =>
            {
                storage.StringValue = value;
            },
            Multiplicity = new ArgumentMultiplicity.SpecificCount(Number: 1, IsRequired: true)
        };
        parser.AddArgument(stringArgument);

        var intListArgument = new Argument<ArgumentConfig, int>
        {
            Description = "Example int list argument",
            ValuePlaceholder = "<int>",
            Converter = ConverterFactory.CreateIntConverter(),
            Action = (storage, value) =>
            {
                storage.IntList.Add(value);
            },
            Multiplicity = new ArgumentMultiplicity.SpecificCount(Number: 2, IsRequired: false)
        };
        parser.AddArgument(intListArgument);

        string[] args = { "-i", "1", "--", "-i", "2", "3" };
        parser.Parse(args);
        Assert.That(config.StringValue, Is.EqualTo("-i"));
        Assert.That(config.IntValue, Is.EqualTo(1));
        Assert.That(config.IntList, Is.EqualTo(new List<int> { 2, 3 }));
    }

    private class RequiredOptionConfig
    {
        public string? StringValue { get; set; }
    }

    [Test]
    public void Parse_RequiredOption()
    {
        var config = new RequiredOptionConfig();
        var parser = new Parser<RequiredOptionConfig>(config)
        {
            Names = new[]
                {"program"},
            Description = null
        };

        var requiredOption = new Option<RequiredOptionConfig, string>
        {
            Names = new[]
            {
                "-r",
                "--required"
            },
            Description = "Example required option",
            Action = (storage, value) =>
            {
                storage.StringValue = value;
            },
            Converter = ConverterFactory.CreateStringConverter(),
            IsRequired = true
        };

        parser.AddOption(requiredOption);

        string[] args = { "--required", "foo" };
        parser.Parse(args);
        Assert.That(config.StringValue, Is.EqualTo("foo"));

        args = Array.Empty<string>();
        Assert.Throws<ParserRuntimeException>(() => parser.Parse(args));
    }
}
