﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Argparse;

public partial record class Parser<C>
{
    private const char valueSeparator = '=';

    [GeneratedRegex("^-{1,2}[a-zA-Z1-9]+[a-zA-Z1-9-]*$")]
    private static partial Regex LongOrShortName();

    List<Flag<C>> flags = new();
    List<IOption<C>> options = new();
    List<IArgument<C>> plainArguments = new();

    Dictionary<string, IOption<C>> optionsMap = new();
    Dictionary<string, Flag<C>> flagsMap = new();

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
            // TODO subcommands
            this.run(args, localRun);
        }
        else
        {
            this.run(args, localRun);
        }
    }

    private void run(string[] args, Action<C, Parser<C>> localRun)
    {
        if (Config is null)
        {
            if (ConfigFactory is null)
                throw new InvalidParserConfigurationException("Config and ConfigFactory are both null. This should never happen");
            Config = ConfigFactory();
        }
        parse(args);

        localRun(Config, this);
    }

    private void parse(string[] args)
    {
        Debug.Assert(Config is not null);

        HashSet<IOption<C>> alreadyParsedOptions = new();
        HashSet<Flag<C>> alreadyParsedFlags = new();

        Action<C> execute = c => { };

        bool encounteredSeparator = false;
        var argsl = args.AsEnumerable();

        while (argsl.Count() > 0)
        {
            var token = argsl.First();
            argsl = argsl.Skip(1);

            if (encounteredSeparator)
                parsePlainArg(token, argsl);
            else if (token == this.PlainArgumentsDelimiter)
                encounteredSeparator = true;
            else if (token.StartsWith("--"))
                argsl = parseLongName(token, argsl);
            else if (token.StartsWith("-"))
                argsl = parseShortOpts(token, argsl);
            else
                parsePlainArg(token, argsl);
        }

        execute(Config);

        void invokeFlag(string token)
        {
            Debug.Assert(flagsMap.ContainsKey(token));
            var flg = flagsMap[token];
            if (alreadyParsedFlags.Contains(flg))
                throw new ParserRuntimeException($"Repeated option: {token}");
            alreadyParsedFlags.Add(flg);

            execute += flg.Action;
        }

        void invokeOption(string token, string value)
        {
            Debug.Assert(optionsMap.ContainsKey(token));
            var opt = optionsMap[token];
            if (alreadyParsedOptions.Contains(opt))
                throw new ParserRuntimeException($"Repeated option: {token}");
            alreadyParsedOptions.Add(opt);

            execute += (c) => opt.Process(c, value);
        }

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

            string opts = token.Substring(1, i - 1);
            string? value = i == token.Length ? null :
                    token.Substring(i + 1, token.Length - i - 1);

            for (int u = 0; u < opts.Length; u++)
            {
                var shortname = $"-{opts[u]}";
                if (u < opts.Length - 1)
                {
                    if (!flagsMap.ContainsKey(shortname))
                        throw new ParserRuntimeException(
                            $"Unknown option: {shortname}");
                    invokeFlag(shortname);
                }
                // option of last index
                else if (flagsMap.ContainsKey(shortname) && value is null)
                    invokeFlag(shortname);
                else if (flagsMap.ContainsKey(shortname) && value is not null)
                    throw new ParserRuntimeException(
                           $"Option does not take value: {shortname}, {value}");
                else if (!optionsMap.ContainsKey(shortname))
                    throw new ParserRuntimeException(
                        $"Unknown option: {shortname}");
                else
                {
                    if (value is null)
                    {
                        if (remainingTokens.Count() == 0)
                            throw new ParserRuntimeException(
                            $"Option requires value: {shortname}");
                        value = remainingTokens.First();
                        remainingTokens = remainingTokens.Skip(1);
                    }
                    invokeOption(shortname, value);
                }

            }

            return remainingTokens;
        }

        IEnumerable<string> parseLongName(string token, IEnumerable<string> remainingTokens)
        {
            Debug.Assert(Config is not null);

            if (token.Contains(valueSeparator))
            {
                var (name, value) = token.Split(valueSeparator, 2);
                if (!optionsMap.ContainsKey(name))
                    throw new ParserRuntimeException($"Unknown option: {name}");
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

        IEnumerable<string> parsePlainArg(string tokens, IEnumerable<string> remainingTokens)
        {
            //Todo
            return remainingTokens;
        }
    }



    private void checkConflictingOptionsAndFlags(string[] names, object what)
    {
        if (names.ToHashSet().Count != names.Length)
        {
            // check duplicates in names discover invalid setups
            HashSet<string> namesFound = new();
            foreach (var name in names)
            {
                if (namesFound.Contains(name))
                    throw new InvalidParserConfigurationException(
                        $"Duplicate name \"{name}\" for {what}.");
                namesFound.Add(name);
            }
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

    private void validateOptionNames(IOption<C> option)
    {
        if (option.Names.Length == 0)
            throw new InvalidParserConfigurationException(
                   $"Option has no name: {options}");

        var invalidOptionNames = option.Names.Where(name => !LongOrShortName().IsMatch(name));
        if (invalidOptionNames.Any())
            throw new InvalidParserConfigurationException($"Invalid flag names: {string.Join(", ", invalidOptionNames)}");

        checkConflictingOptionsAndFlags(option.Names, option);
    }

    private void validateFlagNames(Flag<C> flag)
    {
        if (flag.Names.Length == 0)
            throw new InvalidParserConfigurationException(
                   $"Flag has no name: {flag}");

        var invalidFlagNames = flag.Names.Where(name => !LongOrShortName().IsMatch(name));
        if (invalidFlagNames.Any())
            throw new InvalidParserConfigurationException($"Invalid flag names: {string.Join(", ", invalidFlagNames)}");

        checkConflictingOptionsAndFlags(flag.Names, flag);
    }
    /// <summary>
    /// Checks if the argument is valid.
    ///
    /// An argument is valid if it does not follow an argument with multiplicity AllThatFollow.
    /// </summary>
    /// <exception cref="InvalidParserConfigurationException">If the argument is invalid.</exception>
    private void validateArgument(IArgument<C> argument)
    {
        if (plainArguments.Any())
        {
            var lastArg = plainArguments.Last();
            if (lastArg.Multiplicity is ArgumentMultiplicity.AllThatFollow)
                throw new InvalidParserConfigurationException(
                    $"Argument {argument} cannot follow an argument with multiplicity set to AllThatFollow (argument: {lastArg}).");
        }
    }


    public partial Parser<C> AddArguments(params IArgument<C>[] arguments)
    {
        foreach (var a in arguments) AddArgument(a);
        return this;
    }
    public partial Parser<C> AddArgument(IArgument<C> argument)
    {
        validateArgument(argument);
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
        validateOptionNames(option);
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
        validateFlagNames(flag);
        flags.Add(flag);
        foreach (var name in flag.Names)
            flagsMap.Add(name, flag);

        return this;
    }


}
