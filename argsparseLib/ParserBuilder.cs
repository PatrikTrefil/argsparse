using System;

namespace Argsparse;

public class ParserBuilder<C>
    where C : ParserContext
{

    public ParserBuilder<C> WithHelp(params string[] help)
    {
        return this;
    }

    public ParserBuilder<C2> WithCommand<C2>(string name)
        where C2 : SubcommandParserContext
    {
        return new ParserBuilder<C2>();
    }

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

    public ArgumentBuilder<C, V> WithArgument<V>(string name)
    {
        return new ArgumentBuilder<C, V>(this);
    }

    public ParserBuilder<C> WithArgument<V>(ArgumentBuilder<C, V> argument)
    {
        return this;
    }

    public ParserBuilder<C> Does(Action<C> action)
    {
        return this;
    }


    protected ParserBuilder<C>? parentBuilder;


    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="InvalidBuilderContextTraversalException">
    /// 
    /// </exception>
    /// <returns></returns>
    public ParserBuilder<C> Furthermore()
    {
        /* Information whether there is a parent parser cannot be 
         * added in a subtype - the parent methods would have bad 
         * return types, eg. if we had SubparserBuilder and called
         * WithOption on it, we would get return value of Parserbuilder
         * and could not call Furthermore on it.
         */
        /* Moreover, the information, whether we have a subparser, cannot 
         * be part of the type - we could have something like OptionBuilder
         * and FluentOptionBuilder with a parent, however this would result
         * in code duplication and bad mantainability.
         */

        return this.parentBuilder;
    }

    public Parser<C> Build()
    {
        return null;
    }

    public ParserResult Parse(string input)
    {
        // convenience method
        var parser = this.Build();
        return parser.Parse(input);
    }

    // todo add other convenience methods
}
