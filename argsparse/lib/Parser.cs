using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Argparse;

public partial record class Parser<C>
{
    private const char valueSeparator = '=';

    [GeneratedRegex("(^--[a-zA-Z1-9]+[a-zA-Z1-9-]*$)|(^-[a-zA-Z]$)")]
    private static partial Regex LongOrShortName();


    [GeneratedRegex("^-([a-zA-Z]+)|-([a-zA-Z]=.*)$")]
    private static partial Regex ShortOptionPassed();

    [GeneratedRegex("^--[a-zA-Z1-9]+[a-zA-Z1-9-]*(=.*)?$")]
    private static partial Regex LongOptionPassed();


    List<Flag<C>> flags = new();
    List<IOption<C>> options = new();
    List<IArgument<C>> plainArguments = new();

    Dictionary<string, IOption<C>> optionsMap = new();
    Dictionary<string, Flag<C>> flagsMap = new();

    Dictionary<string, IParser> subparsers = new();

    public partial Parser<C> AddSubparser(string command, IParser commandParser)
    {
        subparsers.Add(command, commandParser);
        return this;
    }

    partial void ParseAndRun(string[] args, bool isRoot, Action<C, Parser<C>> localRun)

    {
        if (isRoot)
        {
            // determine command and then find parser and run ParseAndRun on
            // that parser with the commands removed from the arguments
            //ParseAndRun(..., false);
            // TODO subcommands
            this.RunParser(args, localRun);
        }
        else
        {
            this.RunParser(args, localRun);
        }
    }

    /// <summary>
    /// Run the parser. Invoking this methods starts the parsing process.
    /// If there are subparsers configured,
    /// this method should only be invoked on the one selected parsers
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <param name="localRun">Action to run after parsing</param>
    /// <exception cref="InvalidParserConfigurationException">If there is no config instance nor factory</exception>
    /// <exception cref="ParserRuntimeException">On invalid input</exception>
    private void RunParser(string[] args, Action<C, Parser<C>> localRun)
    {
        if (Config is null)
        {
            if (ConfigFactory is null)
                throw new InvalidParserConfigurationException("Config and ConfigFactory are both null. This should never happen");
            Config = ConfigFactory();
        }
        ParseImpl(args);

        localRun(Config, this);
    }

    /// <summary>
    /// Parse the command line arguments and store the values in the config instance.
    /// The config instance needs to be instantiated before calling this method.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <exception cref="ParserRuntimeException">On invalid input</exception>
    private void ParseImpl(string[] args)
    {
        Debug.Assert(Config is not null);

        HashSet<IOption<C>> alreadyParsedOptions = new();
        HashSet<Flag<C>> alreadyParsedFlags = new();
        Dictionary<IArgument<C>, int> argValueCounts = new();
        // init argValueCounts
        foreach (var arg in plainArguments)
            argValueCounts.Add(arg, 0);

        Action<C> execute = c => { };

        bool encounteredSeparator = false;
        var argsl = args.AsEnumerable();
        // this calls the explicit implementation of IEnumerable<T>, because of the variable type
        IEnumerator<IArgument<C>> plainArgEnumerator = plainArguments.GetEnumerator();
        plainArgEnumerator.MoveNext(); // move to first element

        while (argsl.Count() > 0)
        {
            var token = argsl.First();
            argsl = argsl.Skip(1);

            if (encounteredSeparator)
            {
                argsl = parsePlainArg(token, argsl, plainArgEnumerator);
            }
            else if (token == this.PlainArgumentsDelimiter)
                encounteredSeparator = true;
            else if (token.StartsWith("--") && LongOptionPassed().IsMatch(token))
                argsl = parseLongName(token, argsl);
            else if (token.StartsWith("-") && ShortOptionPassed().IsMatch(token))
                argsl = parseShortOpts(token, argsl);
            else
            {
                argsl = parsePlainArg(token, argsl, plainArgEnumerator);
            }
        }

        checkAllRequiredHaveBeenParsed();

        // execute(Config);

        /// Carry out action associated with the flag identified by the name 
        /// The token must be a valid flag name
        void invokeFlag(string token)
        {
            Debug.Assert(flagsMap.ContainsKey(token));
            var flg = flagsMap[token];
            if (alreadyParsedFlags.Contains(flg))
                throw new ParserRuntimeException($"Repeated option: {token}");
            alreadyParsedFlags.Add(flg);


            // execute += flg.Action;
            flg.Action(Config);
        }

        /// Carry out action associated with the option identified by the name 
        /// The token must be a valid option name
        void invokeOption(string token, string value)
        {
            var opt = optionsMap[token];
            if (alreadyParsedOptions.Contains(opt))
                throw new ParserRuntimeException($"Repeated option: {token}");
            alreadyParsedOptions.Add(opt);

            // execute += (c) => opt.Process(c, value);
            opt.Process(Config, value);
        }

        /// Parse token containing short-named options, e.g. "-a", "-abc".
        /// Merging flags and option is forbidden, e.g. "-abc=foo", "-abc." throws runtime error.
        /// Accepts remaining tokens and consumes up to one of them,
        /// when the last arg recognized in token is option/accepts value.
        /// Returns remaining tokens with up to one token consumed, removed.
        IEnumerable<string> parseShortOpts(string token, IEnumerable<string> remainingTokens)
        {
            Debug.Assert(Config is not null);
            int i = 1;
            for (; i < token.Length; i++)
            {
                var shortname = $"-{token[i]}";
                if (flagsMap.ContainsKey(shortname) || optionsMap.ContainsKey(shortname))
                    continue;
                break;
            }

            if (token.Contains('='))
            {
                var (name, value) = token.Split('=', 2);
                if (name.Length > 2)
                    throw new ParserRuntimeException($"Merging options that take values is not allowed: {token}\n" +
                    $"Use separate options: {token[0..^1]} -{token[^1]}={value}");
                if (flagsMap.ContainsKey(name))
                    throw new ParserRuntimeException(
                           $"Option does not take value: {name}, {value}");
                if (!optionsMap.ContainsKey(name))
                    throw new ParserRuntimeException($"Unknown option: {name}");
                if (value.Length == 0)
                    throw new ParserRuntimeException($"Option value missing: {token}");
                invokeOption(name, value);
            }

            else if (token.Length > 2)
            {
                foreach (char o in token[1..])
                {
                    var name = $"-{o}";
                    if (!flagsMap.ContainsKey(name))
                        throw new ParserRuntimeException($"Unknown option: {name}");
                    invokeFlag(name);
                }
            }

            // now we know token matches form -x
            else
            {
                if (flagsMap.ContainsKey(token))
                    invokeFlag(token);
                else if (optionsMap.ContainsKey(token))
                {
                    if (remainingTokens.Count() == 0)
                        throw new ParserRuntimeException(
                        $"Option requires value: {token}");
                    var value = remainingTokens.First();
                    remainingTokens = remainingTokens.Skip(1);
                    invokeOption(token, value);
                }
                else
                    throw new ParserRuntimeException($"Unknown option: {token}");
            }

            return remainingTokens;
        }

        /// Parse token possibly containing a long-name,  
        IEnumerable<string> parseLongName(string token, IEnumerable<string> remainingTokens)
        {
            Debug.Assert(Config is not null);

            if (token.Contains(valueSeparator))
            {
                var (name, value) = token.Split(valueSeparator, 2);
                if (!optionsMap.ContainsKey(name))
                    throw new ParserRuntimeException($"Unknown option: {name}");
                if (value.Length == 0)
                    throw new ParserRuntimeException($"Option value missing: {token}");
                invokeOption(name, value);
                return remainingTokens;
            }
            else if (flagsMap.ContainsKey(token))
            {
                invokeFlag(token);
                return remainingTokens;
            }
            else if (optionsMap.ContainsKey(token))
            {
                if (remainingTokens.Count() == 0)
                    throw new ParserRuntimeException($"Option requires value: {token}");
                var value = remainingTokens.First();
                invokeOption(token, value);
                return remainingTokens.Skip(1);
            }
            else if (false)
            {
                // maybe try to parse it as plain argument?
            }

            throw new ParserRuntimeException($"Unknown option: {token}");
        }

        IEnumerable<string> parsePlainArg(string token, IEnumerable<string> remainingTokens, IEnumerator<IArgument<C>> argEnumerator)
        {
            Debug.Assert(Config is not null);

            switch (argEnumerator.Current.Multiplicity)
            {
                case ArgumentMultiplicity.SpecificCount argMulSpecificCount:
                    if (argValueCounts[argEnumerator.Current] == argMulSpecificCount.Number)
                    {
                        bool success = argEnumerator.MoveNext();
                        if (!success)
                            throw new ParserRuntimeException("Too many arguments");
                    }
                    break;
                case ArgumentMultiplicity.AllThatFollow:
                    break;
                default:
                    throw new ParserRuntimeException("Unknown multiplicity type: " + argEnumerator.Current.Multiplicity);
            }

            argValueCounts[argEnumerator.Current]++;

            // execute += (c) => argEnumerator.Current.Process(c, token);

            argEnumerator.Current.Process(Config, token);

            return remainingTokens;
        }

        void checkAllRequiredHaveBeenParsed()
        {
            List<object> missingOpts = new();

            missingOpts.AddRange(Options.Where(o => o.IsRequired && !alreadyParsedOptions.Contains(o)));

            if (missingOpts.Count > 0)
                throw new ParserRuntimeException("One or more required options was not specified:\n"
                    + String.Join('\n', missingOpts));

            List<object> missingArgs = new();

            missingArgs.AddRange(Arguments.Where(a =>
            {
                switch (a.Multiplicity)
                {
                    case ArgumentMultiplicity.SpecificCount argMulSpecificCount:
                        return argMulSpecificCount.IsRequired is true && argValueCounts[a] < argMulSpecificCount.Number;
                    case ArgumentMultiplicity.AllThatFollow argMulAllThatFollow:
                        return argValueCounts[a] < argMulAllThatFollow.MinimumNumberOfArguments;
                    default:
                        throw new ParserRuntimeException("Unknown multiplicity type: " + a.Multiplicity);
                }
            }));

            if (missingArgs.Any())
                throw new ParserRuntimeException("One or more required arguments was not specified:\n"
                    + String.Join('\n', missingArgs));
        }
    }


    private void CheckConflictingOptionsAndFlags(string[] names, object what)
    {
        if (names.ToHashSet().Count != names.Length)
        {
            // check duplicates in names discover invalid setups
            HashSet<string> namesFound = new();
            HashSet<string> duplicateNames = new();
            foreach (var name in names)
            {
                if (namesFound.Contains(name))
                    duplicateNames.Add(name);

                namesFound.Add(name);
            }
            if (duplicateNames.Count > 0)
                throw new InvalidParserConfigurationException(duplicateNames.Count == 1 ?
                        $"Duplicate name \"{duplicateNames.First()}\" for {what}."
                        : $"Duplicate names: {String.Join(',', duplicateNames)} for {what}."
                        );
        }
        foreach (var name in names)
        {

            if (optionsMap.ContainsKey(name))
            {
                var conflictingOpt = optionsMap[name];
                throw new InvalidParserConfigurationException(
                    $"Name conflicts with an existing option: {conflictingOpt} conflicts" +
                    $"with {what}.");
            }
            if (flagsMap.ContainsKey(name))
            {
                var conflictingFlag = flagsMap[name];
                throw new InvalidParserConfigurationException(
                    $"Name conflicts with an existing flag: {conflictingFlag}  conflicts" +
                    $"with {what}.");
            }
        }
    }

    private void ValidateOptionNames(IOption<C> option)
    {
        if (option.Names.Length == 0)
            throw new InvalidParserConfigurationException(
                   $"Option has no name: {options}");

        var invalidOptionNames = option.Names.Where(name => !LongOrShortName().IsMatch(name));
        if (invalidOptionNames.Any())
            throw new InvalidParserConfigurationException($"Invalid flag names: {string.Join(", ", invalidOptionNames)}");

        CheckConflictingOptionsAndFlags(option.Names, option);
    }

    private void ValidateFlagNames(Flag<C> flag)
    {
        CheckConflictingOptionsAndFlags(flag.Names, flag);
    }
    /// <summary>
    /// Checks if the argument is valid.
    ///
    /// An argument is valid if it does not follow an argument with multiplicity AllThatFollow.
    /// </summary>
    /// <exception cref="InvalidParserConfigurationException">If the argument is invalid.</exception>
    private void ValidateArgument(IArgument<C> argument)
    {
        if (plainArguments.Any())
        {
            if (plainArguments.Contains(argument))
                throw new InvalidParserConfigurationException(
                    $"Argument {argument} is already added to the parser.");

            var lastArg = plainArguments.Last();
            if (lastArg.Multiplicity is ArgumentMultiplicity.AllThatFollow)
                throw new InvalidParserConfigurationException(
                    $"Argument {argument} cannot follow an argument with multiplicity set to AllThatFollow (argument: {lastArg}).");

            if (lastArg.Multiplicity is ArgumentMultiplicity.SpecificCount argMulSpecificCount && argMulSpecificCount.IsRequired is false)
                throw new InvalidParserConfigurationException(
                    $"Argument {argument} can not follow a non-required argument (argument: {lastArg})."
                );
        }
    }


    public partial Parser<C> AddArguments(params IArgument<C>[] arguments)
    {
        foreach (var a in arguments) AddArgument(a);
        return this;
    }
    public partial Parser<C> AddArgument(IArgument<C> argument)
    {
        ValidateArgument(argument);
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
        ValidateOptionNames(option);
        options.Add(option);
        foreach (var name in option.Names)
            optionsMap.Add(name, option);
        return this;
    }
    public partial Parser<C> AddFlags(params Flag<C>[] flags)
    {
        foreach (var f in flags) AddFlag(f);
        return this;
    }
    public partial Parser<C> AddFlag(Flag<C> flag)
    {
        ValidateFlagNames(flag);
        flags.Add(flag);
        foreach (var name in flag.Names)
            flagsMap.Add(name, flag);

        return this;
    }


}
