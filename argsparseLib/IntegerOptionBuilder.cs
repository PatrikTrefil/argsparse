namespace Argsparse;

public class IntegerOptionBuilder<C> : ValueOptionBuilder<C, int>
    where C : ParserContext
{
    private static int Convert(string s)
    {
        // convert string to int
        return int.Parse(s);   
    }

    protected IntegerOptionBuilder(params string[] names) : base(names, Convert)
    {

    }


    // following methods would require adding validation before calling Does, with Validate interface
    // taking in already converted value and returning bool or void with exceptions

    // public IntegerOptionBuilder<C> Maximum(int maximumInclusive) {}
    // public IntegerOptionBuilder<C> Minimum(int minimumInclusive) {}
    // public IntegerOptionBuilder<C> Between(int minInclusive, int maxInclusive) {}
    // + exclusive or more complex variants
}
