namespace Argsparse.Examples;

class InferenceExample
{
    
    class MyApp : ParserContext
    {
        
    }

    public void Run()
    {

        //// Works
        //var parser = Parser<MyApp>.Create()
        //    .WithOption(OptionBuilder<MyApp>.Create());

        //// Doesnt
        //var parser2 = Parser<MyApp>.Create()
        //    .WithOption(OptionBuilder<?>.Create());

        //var parser3 = Parser<MyApp>.Create()
        //    .WithOption(OptionBuilder<>.Create());

        //var parser4 = Parser<MyApp>.Create()
        //    .WithOption(OptionBuilder.Create());

        //// Idea (ugly)
        //var parser = Parser<MyApp>.Create()
        //    .WithOption("bla", optionBulder => optionBulder.SynonymousWith("blabla")
        //                                                   .Does(a => a.save()))
        //    .WithOption("long", optionBulder => optionBulder.SynonymousWith("l")
        //                                                   .Does(a => a.something()));
    }
}
