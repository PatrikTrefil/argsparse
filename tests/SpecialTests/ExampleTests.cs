using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgparseTests;


class ExampleTests
{
    [Test]
    [TestCase("")]
    [TestCase("--help")]
    public void NumactlExampleDoesNotFail(string args)
    {
        var argsSplit = args.Split(' ');
        Numactl.Program.Main(argsSplit);
    }

    [Test]
    [TestCase("")]
    [TestCase("--help")]
    public void ComplexExampleDoesNotFail(string args)
    {
        var argsSplit = args.Split(' ');
        Argparse.Examples.ComplexExample.Run(argsSplit);
    }

    [Test]
    [TestCase("")]
    [TestCase("--help")]
    public void SimpleExampleDoesNotFail(string args)
    {
        var argsSplit = args.Split(' ');
        Argparse.Examples.SimpleExample.Run(argsSplit);

    }

    [Test]
    [TestCase("")]
    [TestCase("--help")]
    public void TimeExampleDoesNotFail(string args)
    {
        var argsSplit = args.Split(' ');
        Argparse.Examples.TimeExample.Run(argsSplit);
    }
}
