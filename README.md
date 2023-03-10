Argsparse {#mainpage}
=========

This is a simple implementation of a command-line argument parser in C#.

The code defines three main classes: Argument, Flag and Option. An Argument is a value that appears in the command-line arguments list without being associated with a name (e.g., myprogram arg1 arg2). An Option is an argument that is associated with a name and a value (e.g., myprogram --output=outputfile.txt). A Flag is an option that does not take a value (e.g., myprogram --verbose).

All Argument, Flag and Option are defined using C# records, which are classes that can be used to define immutable objects with a concise syntax. The code defines interfaces for IArgument and IOption that are implemented by the Argument and Option classes, respectively. These interfaces define the basic properties that both classes should have: a name, a description, and a way to process their value and provide a handle for all the respective instances irrespective of their value type parameter.

The ArgumentMultiplicity class is used to define the multiplicity of an argument (i.e., how many times it appears in the argument list). It can be either a specific count (e.g., an argument that appears exactly three times) or "all that follow" (e.g., an argument that takes all the remaining arguments).

The OptionFactory class provides some helper methods to create instances of Option with pre-defined behavior, such as a CreateStringOption method that creates an option that takes a string value.

The Parser class is the main class of the library. It is used to define the arguments, flags and options that the program accepts, and to parse the command-line arguments. The Parser is parametrized The Parser class defines a fluent interface that allows the user to define the arguments, flags and options in a concise way. The Parser class also defines a Parse method that takes the command-line arguments which it then parses and invokes the appropriate callbacks for each argument, flag and option.

The interface IParser defines the methods that the Parser class implements. The interface is used to provide a handle for all parsers, irrespective of their config context type argument.

## Architecture decisions {#architecture_decisions}

For some notes on the design decisions,
see [architecture_decisions](architecture_decisions.md).

## An example {#example}

We define the following configuration class as "config context" for the parser:


```csharp
internal record ExampleConfiguration
    {
        public string? algorithm;
        public bool help = false;
    }
```

Then we define the parser and run on console arguments:

```csharp

static void SimpleExample(string[] args)
{
    var config = new ExampleConfiguration();

    var parser = new Parser<ExampleConfiguration>(config)
    {
        Name = "program",
        Description = "Program description"
    };

    var helpFlag = new Flag<ExampleConfiguration>()
    {
        Names = new string[] { "-h", "--help" },
        Description = "Show help",
        Action = (storage) => { storage.help = true; }
    };

    parser.AddFlag(helpFlag);

    var algorithmOption = OptionFactory.CreateStringOption<ExampleConfiguration>() with
    {
        Names = new string[] { "-a", "--algorithm" },
        Description = "Set algorithm to use",
        Action = (storage, value) => { storage.algorithm = value; }
    };

    parser.AddOption(algorithmOption);

    parser.Parse(args);
}
```
For more examples see the [examples](examples) folder.

## Instructions for building

The project uses the C# 6.0. To build the project, run the following command:

```bash
dotnet build
```
### Building documentation

To build the documentation with Doxygen, run the following command:

```bash
doxygen
```

For more information, see [Doxygen manual](https://www.doxygen.nl/manual/index.html).
