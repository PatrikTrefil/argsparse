using System;

namespace Argsparse;

public class ParserBuilder<C>
    where C : ParserContext
{
    public string[] Help { get; }

    public ParserBuilder(Func<C> contextFactory)
    {
    }

    public ParserBuilder(C contextInstance)
    {
    }

    public ParserBuilder<C> WithHelpText(params string[] help)
    {
        return this;
    }

    public ParserBuilder<C> WithoutDefaultHelpOption()
    {
        return this;
    }
    
    /// <summary>
    /// Use this option instead to show help.
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    public ParserBuilder<C> WithHelpOption(OptionBuilder<C> helpOption)
    {
        return this;
    }

    // We decided to not allow fluent adding of subparsers
    // as not permit confusing multiline monstrosities
    //public ParserBuilder<C2> WithCommand<C2>(string name)
    //    where C2 : SubcommandParserContext
    //{
    //    return new ParserBuilder<C2>();
    //}

    public ParserBuilder<C> WithCommand<C2>(ParserBuilder<C2> command)
        where C2 : SubcommandParserContext
    {
        return this;
    }
    
    public OptionBuilder<C> WithOption(params string[] names)
    {
        return new OptionBuilder<C>(this);
    }

    public ParserBuilder<C> WithOption(OptionBuilder<C> option)
    {
        return this;
    }

    public CustomArgumentBuilder<C, V> WithArgument<V>(string name)
    {
        return new CustomArgumentBuilder<C, V>(this);
    }

    public ParserBuilder<C> WithArgument<V>(CustomArgumentBuilder<C, V> argument)
    {
        return this;
    }

    public ParserBuilder<C> Runs(Action<C> action)
    {
        return this;
    }

    public Parser<C> Build()
    {
        return null;
    }

        // convenience method
    public ParserResult Parse(string input)
    {
        var parser = this.Build();
        return parser.Parse(input);
    }
    
}
