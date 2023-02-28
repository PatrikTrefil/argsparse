using System;

namespace Argsparse;

public class ArgumentBuilder<C, V>
    where C : ParserContext
{
    ParserBuilder<C>? parentParser;

    public ArgumentBuilder(ParserBuilder<C> parentParser)
    {
        this.parentParser = parentParser;
    }

    public ArgumentBuilder()
    {
        this.parentParser = null;
    }


    public ArgumentBuilder<C, V> WithHelp(string help)
    {
        return this;
    }

    public ArgumentBuilder<C, V> Default(V defaultValue)
    {
        return this;
    }

    public ArgumentBuilder<C, V> Optional()
    {
        return this;
    }

    public ArgumentBuilder<C, V> Does(Action<C, V> action)
    {
        return this;
    }

    /// <exception cref="InvalidBuilderContextTraversalException">
    /// 
    /// </exception>
    public ParserBuilder<C> And()
    {
        return this.parentParser;
    }
}