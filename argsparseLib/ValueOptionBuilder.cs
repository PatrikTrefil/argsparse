using System;

namespace Argsparse;

/// <summary>
/// Builder representing option with string value
/// </summary>
/// <typeparam name="C">Parser context</typeparam>
public sealed class ValueOptionBuilder<C> : ValueOptionBuilder<C, string>
    where C : ParserContext
{
    public static ValueOptionBuilder<C> Create(params string[] names)
    {
        return (new ValueOptionBuilder<C>(names).Converts(s => s) as ValueOptionBuilder<C>)!;
    }
    private ValueOptionBuilder(params string[] names) : base(names)
    {
    }

    // Why all the new methods?
    // We want to keep the generic ValueOptionBuilder for possible further extension
    // And for user to define options with custom conversion
    // But we need the methods to return the same type so as not to confuse the user
    // and keep all the possible added methods (such as Maximum on IntegerOption)
    // available in the fluent syntax.

    public new ValueOptionBuilder<C> WithHelpText(string help)
    {
        return (base.WithHelpText(help) as ValueOptionBuilder<C>)!;
    }

    public new ValueOptionBuilder<C> SynonymousWith(params string[] names)
    {
        return (base.SynonymousWith(names) as ValueOptionBuilder<C>)!;
    }

    public new ValueOptionBuilder<C> Default(string value)
    {
        return (base.Default(value) as ValueOptionBuilder<C>)!;
    }

    public new ValueOptionBuilder<C> Does(Action<C, string> what)
    {
        return (base.Does(what) as ValueOptionBuilder<C>)!;
    }

    // Fluent syntax

    public ValueOptionBuilder(OptionBuilder<C> parentOptionBuilder) : base(parentOptionBuilder) { }
}

// Keep this generic variant here, if we decide to extend to support conversion
public class ValueOptionBuilder<C, V> 
where C : ParserContext
{
    // Here we choose composition over inheritance so that we don't have to add
    // code to copy values from OptionBuilder into ValueOptionBuilder when we
    // decide to do "WithValue" in fluent syntax

    protected OptionBuilder<C>? parentOptionBuilder;

    // Protected
    protected ValueOptionBuilder(params string[] names)
    {
     
    }

    protected ValueOptionBuilder(string[] names, Func<string, V> convert)
    {

    }

    public ValueOptionBuilder<C, V> WithHelpText(string help)
    {
        return this;
    }

    public ValueOptionBuilder<C, V> SynonymousWith(params string[] names)
    {
        return this;
    }

    public ValueOptionBuilder<C, V> Default(V value)
    {
        return this;
    }

    public ValueOptionBuilder<C, V> Does(Action<C, V> what)
    {
        return this;
    }

    public ValueOptionBuilder<C, V> Converts(Func<string, V> conversionFunction)
    {
        return this;
    }

    // Fluent syntax

    public ValueOptionBuilder(OptionBuilder<C> parentOptionBuilder)
    {
        this.parentOptionBuilder = parentOptionBuilder;
    }

    /// <exception cref="InvalidBuilderContextTraversalException">
    /// 
    /// </exception>
    public ParserBuilder<C> And() => parentOptionBuilder.And();
}
