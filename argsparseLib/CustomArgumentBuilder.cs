using System;

namespace Argsparse;

public class CustomArgumentBuilder<C, V>
    where C : ParserContext
{
    ParserBuilder<C>? parentParser;
    

    public static CustomArgumentBuilder<C,V> Create()
    {
        return null;
    }

    public static CustomArgumentBuilder<C, V> Create(ParserBuilder<C> parentParser)
    {
        return null;
    }

    protected CustomArgumentBuilder(ParserBuilder<C> parentParser)
    {
        this.parentParser = parentParser;
    }

    protected CustomArgumentBuilder()
    {
        this.parentParser = null;
    }
   
    public CustomArgumentBuilder<C, V> WithHelpText(string help)
    {
        return this;
    }

    public CustomArgumentBuilder<C, V> Default(V defaultValue)
    {
        return this;
    }

    public CustomArgumentBuilder<C, V> Name(string name)
    {
        return this;
    }

    public CustomArgumentBuilder<C, V> Optional()
    {
        return this;
    }

    public CustomArgumentBuilder<C, V> Does(Action<C, V> action)
    {
        return this;
    }

    // Fluent syntax
    
    /// <exception cref="InvalidBuilderContextTraversalException">
    /// 
    /// </exception>
    public ParserBuilder<C> And()
    {
        return this.parentParser;
    }
}