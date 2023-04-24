using System;

namespace Argparse;

/// <summary>
/// Models an expception thrown when the parser is not configured properly, for example with subcommands of the same name.
/// </summary>
public class InvalidParserConfigurationException : Exception
{
    public InvalidParserConfigurationException(string message) : base(message) { }
}

/// <summary>
/// Models an exception thrown when the parser fails to convert a value from string to the intended type.
/// </summary>
public class ParserConversionException : ParserRuntimeException
{
    public ParserConversionException(string message) : base(message) { }

    public ParserConversionException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Models an exception thrown when the parser fails to run the action associated with an option, flag or argument or a parser.
/// </summary>
public class ParserRuntimeException : Exception
{
    public ParserRuntimeException(string message) : base(message) { }

    public ParserRuntimeException(string message, Exception inner) : base(message, inner) { }
}
