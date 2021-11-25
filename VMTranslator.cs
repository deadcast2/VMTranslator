using System;
using System.IO;

namespace VMTranslator
{
    class VMTranslator
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (Directory.Exists(args[0]))
                {
                    var outputFile = Path.Join(Path.GetFullPath(args[0]), Path.GetFileName(args[0]));

                    new Parser(outputFile, Directory.GetFiles(args[0], "*.vm"));
                }
                else
                {
                    new Parser(Path.GetFullPath(args[0]), args);
                }
            }
            else
            {
                Console.WriteLine("A file path or directory must be specified. For example: VMTranslator myProg.vm");
            }
        }
    }
}
