using System;
using System.Collections.Generic;
using System.Linq;

namespace Numactl;
record NumaCtlArgs
{
    public bool? Help { get; set; }
    public List<int>? Interleave { get; set; }
    public int? Preferred { get; set; }
    public List<int>? MemBind { get; set; }
    public List<int>? PhysCpuBind { get; set; }
    public bool? Show { get; set; }
    public bool? Hardware { get; set; }
    public string? Command { get; set; }
    public List<string>? CommandArgs { get; set; }

    public bool HelpMode() => Help != null && (bool)Help;
    public bool ShowMode() => Show != null && (bool)Show;
    public bool HardwareMode() => Hardware != null && (bool)Hardware;
    public bool ExecutionMode() => !HelpMode() && !ShowMode() && !HardwareMode();

    public void AssertValid()
    {
        if (HelpMode())
            AssertValidForHelpMode();
        else if (ShowMode())
            AssertValidForShowMode();
        else if (HardwareMode())
            AssertValidForHardwareMode();
        else if (ExecutionMode())
            AssertValidForExecutionMode();
        else
            throw new NotImplementedException("Validation for the program mode has not been implemented.");
    }

    private void AssertValidForHelpMode()
    {
        if (!ExactlyOneDefined())
            throw new InvalidProgramArgumentsException("If --help is set no other argument/option is allowed");
    }

    private void AssertValidForShowMode()
    {
        if (!ExactlyOneDefined())
            throw new InvalidProgramArgumentsException("If --show flag is set no other argument/option is allowed");
    }

    private void AssertValidForHardwareMode()
    {
        if (!ExactlyOneDefined())
            throw new InvalidProgramArgumentsException("If --hardware flag is set no other argument/option is allowed.");
    }

    private void AssertValidForExecutionMode()
    {
        var valid_m_p_i_options = AtMostOneHolds(
            MemBind != null,
            Preferred != null,
            Interleave != null
        );

        if (!valid_m_p_i_options)
            throw new InvalidProgramArgumentsException("Only one of -m, -p, -i options are allowed.");

        if (Command == null)
            throw new InvalidProgramArgumentsException("You have to specify command to be executed on NUMA architecture.");
    }

    private void AssertValidRanges()
    {
        var rangesAreValid =
            PhysCpuBind.ValuesAreInRange(0, 31) &&
            MemBind.ValuesAreInRange(0, 3) &&
            Interleave.ValuesAreInRange(0, 3) &&
            Preferred.IsInRange(0, 3);


        if (!rangesAreValid)
            throw new InvalidProgramArgumentsException("Invalid ranges of given options.");
    }


    private bool AtMostOneHolds(params bool[] props)
    {
        return props.Where(p => p).Count() <= 1;
    }

    private bool ExactlyOneDefined()
    {
        var notNullValuesCount = typeof(NumaCtlArgs)
            .GetProperties()
            .Select(prop => prop.GetGetMethod()?.Invoke(this, null))
            .Where(propVal => propVal != null)
            .Count();

        return notNullValuesCount == 1;
    }
}
