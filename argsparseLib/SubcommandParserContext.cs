using System;

namespace Argsparse;

public class SubcommandParserContext : ParserContext
{
    ParserContext ParentParserContext { get; }
}
