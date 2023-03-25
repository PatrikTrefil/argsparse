using System;
using System.IO;
using System.Linq;

namespace Argparse;

/// <summary>
/// An object which is used to create help write-ups for parsers, the message one usually sees
/// when a command is passed the --help flag.
/// </summary>
/// <typeparam name="T">The cofiguration context type for the given parser, see <see cref="Parser{C}"/></typeparam>
public interface IParserHelpFormatter<T>
{
    /// <summary>
    /// Prints a help write-up to the provided <see cref="TextWriter"/> from the information
    /// gathered from the provided <paramref name="parser"/>.
    /// </summary>
    public abstract void PrintHelp(Parser<T> parser, TextWriter writer);
}

/// <summary>
/// An implementation of <see cref="IParserHelpFormatter{T}"/> that provides basic
/// help formatting which includes all information about options and arguments
/// </summary>
public sealed class DefaultHelpFormatter<T> : IParserHelpFormatter<T>
{

    public void PrintHelp(Parser<T> parser, TextWriter writer)
    {
        System.Console.WriteLine(parser.Name);

        if (parser.Description is not null)
        {
            System.Console.WriteLine();
            System.Console.WriteLine(parser.Description);
        }

        System.Console.WriteLine();

        Console.Write(parser.Name);
        if (parser.Options.Any() || parser.Flags.Any())
            Console.Write(" [options]");
        if (parser.Arguments.Any())
        {
            foreach (var arg in parser.Arguments)
            {
                switch (arg.Multiplicity)
                {
                    case ArgumentMultiplicity.SpecificCount argMulSpecCount:
                        for (int i = 0; i < argMulSpecCount.Number; i++)
                            Console.Write(" " + arg.ValuePlaceholder);

                        break;
                    case ArgumentMultiplicity.AllThatFollow:
                        Console.Write(" " + arg.ValuePlaceholder + " ...");
                        break;
                }
            }
            Console.WriteLine();
        }

        if (parser.Options.Any())
        {
            Console.WriteLine();
            Console.WriteLine("Options:");
        }

        foreach (var option in parser.Options)
        {
            if (option.Names is null)
                Console.Write("no-names-provided");
            else
                foreach (var name in option.Names)
                {
                    System.Console.Write(name + "=" + option.ValuePlaceHolder);
                    System.Console.Write(", ");
                }

            if (option.Description is not null)
            {
                System.Console.Write("- ");
                System.Console.Write(option.Description);
            }
            Console.WriteLine();
        }

        if (parser.Flags.Any())
        {
            Console.WriteLine();
            Console.WriteLine("Flags:");
        }
        foreach (var flag in parser.Flags)
        {
            if (flag.Names is null)
                Console.Write("no-name-provided");
            else
                foreach (var name in flag.Names)
                {
                    Console.Write(name);
                    Console.Write(", ");
                }

            if (flag.Description is not null)
            {
                Console.Write("- ");
                Console.Write(flag.Description);
            }
            Console.WriteLine();
        }

        if (parser.Arguments.Any())
        {
            Console.WriteLine();
            Console.WriteLine("Arguments:");
        }
        foreach (var arg in parser.Arguments)
        {
            Console.Write(arg.ValuePlaceholder);
            if (arg.Description is not null)
            {
                Console.Write("- ");
                Console.Write(arg.Description);
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }


}
