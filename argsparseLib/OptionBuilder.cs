using System;

namespace Argsparse;

public class OptionBuilder<C>
    where C : ParserContext
{
    
    public string Names { get; }
    public string[] Help { get; }

    public static OptionBuilder<C> Create(string[] names)
    {
        return new OptionBuilder<C>();
    }

    public static OptionBuilder<C> Create()
    {
        return new OptionBuilder<C>();
    }
    
    protected OptionBuilder()
    {
    }

    public OptionBuilder<C> WithHelpText(string help)
    {
        return this;
    }

    public OptionBuilder<C> SynonymousWith(params string[] names)
    {
        return this;
    }

    // We decided to let value conversion up to the consumer
    // Except for some predefined class
    //public ValueOptionBuilder<C, V> WithValue<V>()
    //{
    //    return new ValueOptionBuilder<C, V>(this);
    //}

    public ValueOptionBuilder<C> WithValue()
    {
        return new ValueOptionBuilder<C>(this);
    }

    public OptionBuilder<C> Does(Action<C> action)
    {
        return this;
    }

    // Fluent syntax

    protected ParserBuilder<C>? parentCommandBuilder;

    public OptionBuilder(ParserBuilder<C> parentCommandBuilder)
    {
        this.parentCommandBuilder = parentCommandBuilder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="InvalidBuilderContextTraversalException">
    /// </exception>
    /// <returns></returns>
    public ParserBuilder<C> And()
    {
        return this.parentCommandBuilder;
    }

}
