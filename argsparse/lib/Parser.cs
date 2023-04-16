using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Argparse;

public partial record class Parser<C>
{
    List<Flag<C>> flags = new();
    List<IOption<C>> options = new();
    List<IArgument<C>> plainArguments = new();

    Dictionary<string, IParser> subparsers;

    public partial Parser<C> AddSubparser(string command, IParser commandParser)
    {
        return this;
    }

    partial void ParseAndRun(string[] args, bool isRoot, Action<C, Parser<C>> localRun)

    {
        if (isRoot)
        {
            // determine command and then find parser and run ParseAndRun on
            // that parser with the commands removed from the arguments
            //ParseAndRun(..., false);
        }
        else
        {
            this.run(localRun);
        }
    }

    private void run(Action<C, Parser<C>> localRun)
    {
        if (Config is null)
        {
            if (ConfigFactory is null)
                throw new InvalidParserConfigurationException("Config and ConfigFactory are both null. This should never happen");
            Config = ConfigFactory();
        }
        parse();

        localRun(Config, this);
    }

    private void parse()
    {

    }

    public partial Parser<C> AddArguments(params IArgument<C>[] arguments)
    {
        foreach (var a in arguments) AddArgument(a);
        return this;
    }
    public partial Parser<C> AddArgument(IArgument<C> argument)
    {
        plainArguments.Add(argument);
        return this;
    }
    public partial Parser<C> AddOptions(params IOption<C>[] options)
    {
        foreach (var o in options) AddOption(o);
        return this;
    }
    public partial Parser<C> AddOption(IOption<C> option)
    {
        options.Add(option);
        return this;
    }
    public partial Parser<C> AddFlags(params Flag<C>[] flags)
    {
        foreach (var f in flags) AddFlag(f);
        return this;
    }
    public partial Parser<C> AddFlag(Flag<C> flag)
    {
        flags.Add(flag);
        return this;
    }

}
