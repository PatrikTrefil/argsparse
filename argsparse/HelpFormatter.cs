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
    public abstract void PrintHelp<T2>(IParser<T2> parser, TextWriter writer)
        where T2 : T;

}

/// <summary>
/// An implementation of <see cref="IParserHelpFormatter{T}"/> that provides basic
/// help formatting which includes all information about options and arguments
/// </summary>
public sealed class DefaultHelpFormatter<T> : IParserHelpFormatter<T>
{

    public void PrintHelp<T2>(IParser<T2> parser, TextWriter writer)
        where T2 : T
    {
        if (parser.Names.Length > 0)
            writer.WriteLine(parser.Names[0]);

        if (parser.Description is not null)
        {
            writer.WriteLine();
            writer.WriteLine(parser.Description);
        }

        writer.WriteLine();

        writer.Write(string.Join(',', parser.Names));
        if (parser.Options.Any() || parser.Flags.Any())
            writer.Write(" [options]");
        if (parser.Arguments.Any())
        {
            foreach (var arg in parser.Arguments)
            {
                switch (arg.Multiplicity)
                {
                    case ArgumentMultiplicity.SpecificCount argMulSpecCount:
                        for (int i = 0; i < argMulSpecCount.Number; i++)
                            writer.Write(" " + arg.ValuePlaceholder);

                        break;
                    case ArgumentMultiplicity.AllThatFollow:
                        writer.Write(" " + arg.ValuePlaceholder + " ...");
                        break;
                }
            }
            writer.WriteLine();
        }

        if (parser.Options.Any())
        {
            writer.WriteLine();
            writer.WriteLine("Options:");
        }

        foreach (var option in parser.Options)
        {
            if (option.Names is null)
                writer.Write("no-names-provided");
            else
                foreach (var name in option.Names)
                {
                    writer.Write(name + "=" + option.ValuePlaceHolder);
                    writer.Write(", ");
                }

            if (option.IsRequired)
                writer.Write("(required) ");

            if (option.Description is not null)
            {
                writer.Write("- ");
                writer.Write(option.Description);
            }
            writer.WriteLine();
        }

        if (parser.Flags.Any())
        {
            writer.WriteLine();
            writer.WriteLine("Flags:");
        }
        foreach (var flag in parser.Flags)
        {
            if (flag.Names is null)
                writer.Write("no-name-provided");
            else
                foreach (var name in flag.Names)
                {
                    writer.Write(name);
                    writer.Write(", ");
                }

            if (flag.Description is not null)
            {
                writer.Write("- ");
                writer.Write(flag.Description);
            }
            writer.WriteLine();
        }

        if (parser.Arguments.Any())
        {
            writer.WriteLine();
            writer.WriteLine("Arguments:");
        }
        foreach (var arg in parser.Arguments)
        {
            writer.Write(arg.ValuePlaceholder);

            PrintMultiplicity(arg.Multiplicity, writer);
            
            if (arg.Description is not null)
            {
                writer.Write("- ");
                writer.Write(arg.Description);
            }
            writer.WriteLine();
        }
        writer.WriteLine();

        if(parser.SubParsers.Any())
        {
            writer.WriteLine("Sub-Commands:");
            foreach(var  subParser in parser.SubParsers.Values)
            {
                writer.Write(String.Join(", ", subParser.Names));
                if(subParser.Description is not null)
                {
                    writer.Write(" - ");
                    writer.Write(subParser.Description);
                }
                writer.WriteLine();
            }
        }
        writer.WriteLine();
    }


    private void PrintMultiplicity(ArgumentMultiplicity mult, TextWriter writer) {
         switch(mult)
            {
                case ArgumentMultiplicity.SpecificCount argMulSpecCount when argMulSpecCount.Number == 1 &&  argMulSpecCount.IsRequired:
                    break;
                case ArgumentMultiplicity.SpecificCount argMulSpecCount when argMulSpecCount.Number == 1 &&  !argMulSpecCount.IsRequired:
                    writer.Write(" (optional)");
                    break;
                case ArgumentMultiplicity.SpecificCount argMulSpecCount when argMulSpecCount.Number > 1 :
                    writer.Write(" (x" + argMulSpecCount.Number );
                    if (!argMulSpecCount.IsRequired)
                        writer.Write(", optional)");
                    else 
                        writer.Write(")");
                    break;

                case ArgumentMultiplicity.AllThatFollow argMulAllFollow when argMulAllFollow.MinimumNumberOfArguments == 0:
                    writer.Write(" (any number of arguments)");
                    break;
                case ArgumentMultiplicity.AllThatFollow argMulAllFollow when argMulAllFollow.MinimumNumberOfArguments == 1:
                    writer.Write(" (at least x1)");
                    break;
                    case ArgumentMultiplicity.AllThatFollow argMulAllFollow when argMulAllFollow.MinimumNumberOfArguments < 1:
                    writer.Write($" (at least x{argMulAllFollow.MinimumNumberOfArguments})");
                    break;

                default: throw new NotImplementedException();
            }; 

    }


}
