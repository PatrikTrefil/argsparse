using System;

namespace Numactl;
public class InvalidProgramArgumentsException : Exception
{
    public InvalidProgramArgumentsException(string msg) : base(msg)
    {
    }

}
