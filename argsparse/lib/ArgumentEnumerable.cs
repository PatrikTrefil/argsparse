using System.Collections;
using System.Collections.Generic;

namespace Argparse;

/// <summary>Enumerate arguments in the order they should be parsed.</summary>
/// <example>If there are two provided arguments (referred to as firstArg and secondArg),
/// where the first one is
/// a required argument, which is expected twice and the second one is
/// an argument with multiplicity <see cref="ArgumentMultiplicity.AllThatFollow"/>,
/// then the enumeration will be as follows: firstArg, firstArg, secondArg, secondArg, secondArg, ...
/// (indefinitely)</example>
internal class ArgumentEnumerable<C> : IEnumerable<IArgument<C>>
{
    readonly IEnumerable<IArgument<C>> arguments;
    /// <param name="arguments">Arguments to enumerate over</param>
    public ArgumentEnumerable(IEnumerable<IArgument<C>> arguments) { this.arguments = arguments; }
    public IEnumerator<IArgument<C>> GetEnumerator()
    {
        foreach (var arg in arguments)
        {
            switch (arg.Multiplicity)
            {
                case ArgumentMultiplicity.SpecificCount argMulSpecificCount:
                    for (int i = 0; i < argMulSpecificCount.Number; ++i)
                        yield return arg;

                    break;
                case ArgumentMultiplicity.AllThatFollow:
                    while (true)
                    {
                        yield return arg;
                    }
                default:
                    throw new ParserRuntimeException("Unknown multiplicity type: " + arg.Multiplicity);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
