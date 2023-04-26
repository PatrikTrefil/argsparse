namespace ArgparseTests;

/// <summary>
/// Tests for non-parsing classes
/// Such as <see cref="Flag{T}"/>, <see cref="Argument{T, TArg}"/> and converters
/// </summary>
public class HelperTests
{
    private class TestConfig
    {
        public bool Flag { get; set; }
        public int IntValue { get; set; }
    }

    [Test]
    public void Flag_SetProperties()
    {
        var flag = new Flag<TestConfig>
        {
            Names = new[] { "--flag", "-f" },
            Description = "A flag for testing purposes",
            Action = config => config.Flag = true
        };

        Assert.That(flag.Names, Is.EqualTo(new[] { "--flag", "-f" }));
        Assert.That(flag.Description, Is.EqualTo("A flag for testing purposes"));
        Assert.That(flag.Action, Is.Not.Null);
    }

    [Test]
    public void Argument_SetProperties()
    {
        var arg = new Argument<TestConfig, int>
        {
            Description = "An integer argument for testing purposes",
            ValuePlaceholder = "<int>",
            Converter = ConverterFactory.CreateIntConverter(),
            Action = (config, value) => config.IntValue = value,
            Multiplicity = new ArgumentMultiplicity.SpecificCount(Number: 1, IsRequired: true)
        };

        Assert.That(arg.Description, Is.EqualTo("An integer argument for testing purposes"));
        Assert.That(arg.ValuePlaceholder, Is.EqualTo("<int>"));
        Assert.That(((ArgumentMultiplicity.SpecificCount)arg.Multiplicity).Number, Is.EqualTo(1));
        Assert.IsTrue(((ArgumentMultiplicity.SpecificCount)arg.Multiplicity).IsRequired);
        Assert.That(arg.Converter, Is.Not.Null);
        Assert.That(arg.Action, Is.Not.Null);
    }

    [Test]
    public void Option_SetProperties()
    {
        var option = new Option<TestConfig, int>
        {
            Names = new[] { "--option", "-o" },
            Description = "An integer option for testing purposes",
            Converter = ConverterFactory.CreateIntConverter(),
            Action = (config, value) => config.IntValue = value,
        };

        Assert.That(option.Names, Is.EqualTo(new[] { "--option", "-o" }));
        Assert.That(option.Description, Is.EqualTo("An integer option for testing purposes"));
        Assert.That(option.Converter, Is.Not.Null);
        Assert.That(option.Action, Is.Not.Null);
    }

    [Test]
    public void ArgumentMultiplicity_SetProperties()
    {
        var specificCount = new ArgumentMultiplicity.SpecificCount(3, false);
        var allThatFollow = new ArgumentMultiplicity.AllThatFollow(2);

        Assert.That(specificCount.Number, Is.EqualTo(3));
        Assert.IsFalse(specificCount.IsRequired);
        Assert.That(allThatFollow.MinimumNumberOfArguments, Is.EqualTo(2));
    }

    [Test]
    public void IntConverter_ConvertsProperly()
    {
        var converter = ConverterFactory.CreateIntConverter(0, 10);
        var value = converter("5");
        Assert.That(value, Is.EqualTo(5));
    }

    [Test]
    public void IntConverter_ThrowsException()
    {
        var converter = ConverterFactory.CreateIntConverter(0, 10);

        Assert.Throws<ParserConversionException>(() => converter("-1"));
        Assert.Throws<ParserConversionException>(() => converter("11"));
        Assert.Throws<ParserConversionException>(() => converter("abc"));
    }

    [Test]
    public void ListConverter_ConvertsProperly()
    {
        var converter = ConverterFactory.CreateListConverter<int>(ConverterFactory.CreateIntConverter());
        var list = converter("1,2,3,4,5");

        Assert.That(list, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
    }

    [Test]
    public void ListConverter_ThrowsException()
    {
        var converter = ConverterFactory.CreateListConverter<int>(ConverterFactory.CreateIntConverter());

        Assert.Throws<ParserConversionException>(() => converter("1,2,3,4,abc"));
    }
}
