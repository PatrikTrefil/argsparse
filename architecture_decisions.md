# Architecture decisions

## Options and flags

Options and Flags are two different things. Options have values, flags don't.
There is only one Option and it cannot be inherited from. If we want to create a
new Option with predefined behavior (e.g. conversion), we use OptionFactory
(which avoids inheritance and simplifies things).

## Fluent syntax

All classes are mutable and support fluent syntax using the "with" keyword. All
classes are always valid instances.

Fluent syntax is just one way to use the classes. It is also possible to fill
everything in manually, for example using an object initializer. At the same
time, this allows the user to read the configuration.

Example of object initializer:

```csharp
var flag = new Flag<Config>
{
    Names = new[] { "-h", "--help" },
    Description = "Print help",
    Action = (config, value) => { config.PrintHelp = true; }
};
```

## Help

Printing help can be called directly on the parser, but the print logic is in a
separate class. We provide a default implementation, but the user can write
their own by implementing the ParserHelpFormatter interface.

## Parser config context instance

Individual parsers can store data in an instance that is passed to them by
reference or can receive a factory method and create an instance themselves if
needed (this allows us to avoid a lot of null values - see examples).

## Record classes

We use records because they give us a free print method, copy constructor,
hashing, and equality comparison.
