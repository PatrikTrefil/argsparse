using System.IO;

namespace Argsparse;

public class Parser<C>
where C : ParserContext
{
    public static ParserBuilder<C> Create()
    {
        return new ParserBuilder<C>();
    }

    public ParserResult Parse(string input)
    {
        return default;
    }
    public ParserResult Parse(TextReader input)
    {
        return default;

    }
}

public class Parser : Parser<EmptyParserContext>
{

}
