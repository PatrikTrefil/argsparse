using System;
using System.IO;
using System.Xml.Linq;

namespace Argsparse;

public class Parser<C>
where C : ParserContext
{
    public static ParserBuilder<C> Create()
    {
        return default;
        
        //var constructor = typeof(C).GetConstructor(Type.EmptyTypes); // get parameterless constructor

        //if (constructor == null) throw new Exception("TODO message No default constructor");
        
        //return new ParserBuilder<C>((C) constructor.Invoke(null));
    }

    public static ParserBuilder<C> Create(C contextInstance)
    {
        return new ParserBuilder<C>(contextInstance);
    }

    public static ParserBuilder<C> Create(Func<C> contextFactory)
    {
        return new ParserBuilder<C>(contextFactory);
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
