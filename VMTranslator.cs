using System;

namespace VMTranslator
{
    class VMTranslator
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                new Parser(args[0]);
            }
            else
            {
                Console.WriteLine("A file path must be specified. For example: VMTranslator myProg.vm");
            }
        }
    }
}
