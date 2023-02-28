using System;

namespace Argsparse;

public class OptionBuilder<C, V>
where C : ParserContext
{

    protected OptionBuilder<C>? parentOptionBuilder;
    public OptionBuilder(OptionBuilder<C> parentOptionBuilder)
    {
        this.parentOptionBuilder = parentOptionBuilder;
    }

    public OptionBuilder()
    {
        this.parentOptionBuilder = null;
    }

    public OptionBuilder<C, V> WithHelp(string help)
    {
        return this;
    }

    public OptionBuilder<C, V> SynonymousWith(params string[] names)
    {
        return this;
    }

    /// <exception cref="InvalidBuilderContextTraversalException">
    /// 
    /// </exception>
    public ParserBuilder<C> And() => parentOptionBuilder.And();

    public OptionBuilder<C, V> Default(V value)
    {
        return this;
    }

    public OptionBuilder<C, V> Does(Action<C, V> what)
    {
        return this;
    }
}
