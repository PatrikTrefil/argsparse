using System;

namespace Argsparse;

public class ArgumentBuilder<C> : CustomArgumentBuilder<C, string>
    where C : ParserContext
{
    private ArgumentBuilder(ParserBuilder<C> parentParser) : base(parentParser) { }
    
    protected ArgumentBuilder() : base() { }
    
    public new static ArgumentBuilder<C> Create()
    {
        return null;
    }

    public new static ArgumentBuilder<C> Create(ParserBuilder<C> parentParser)
    {
        return null;
    }


    public new ArgumentBuilder<C> WithHelpText(string help)
    {
        return (base.WithHelpText(help) as ArgumentBuilder<C>)!;
    }

    public new ArgumentBuilder<C> Default(string defaultValue)
    {
        return (base.Default(defaultValue) as ArgumentBuilder<C>)!;
    }

    public new ArgumentBuilder<C> Name(string name)
    {
        return (base.Name(name) as ArgumentBuilder<C>)!;
    }

    public new ArgumentBuilder<C> Optional()
    {
        return (base.Optional() as ArgumentBuilder<C>)!;
    }

    public new ArgumentBuilder<C> Does(Action<C, string> action)
    {
        return (base.Does(action) as ArgumentBuilder<C>)!;
    }
}
