using System;

namespace Argsparse;

public class OptionBuilder<C>
    where C : ParserContext
{

    public static OptionBuilder<C> Create()
    {
        return new OptionBuilder<C>();
    }

    protected ParserBuilder<C>? parentCommandBuilder;

    public OptionBuilder(ParserBuilder<C> parentCommandBuilder)
    {
        this.parentCommandBuilder = parentCommandBuilder;
    }
    public OptionBuilder()
    {
        this.parentCommandBuilder = null;
    }

    public OptionBuilder<C> WithHelp(string help)
    {
        return this;
    }

    public OptionBuilder<C> SynonymousWith(params string[] names)
    {
        return this;
    }

    public OptionBuilder<C, V> WithValue<V>()
    {
        return new OptionBuilder<C, V>(this);
    }

    public OptionBuilder<C> Does(Action<C> what)
    {
        return this;
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
