using System;
using System.IO;

namespace VMTranslator
{
    class VMTranslator
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    if (Directory.Exists(args[0]))
                    {
                        ProcessDirectory(args[0]);
                    }
                    else
                    {
                        new Parser(Path.GetFullPath(args[0]), args);
                    }
                }
                else
                {
                    ProcessDirectory(Directory.GetCurrentDirectory());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void ProcessDirectory(string directory)
        {
            var outputFile = Path.Combine(Path.GetFullPath(directory), Path.GetFileName(TrimEndingDirectorySeparator(directory)));

            new Parser(outputFile, Directory.GetFiles(directory, "*.vm"));
        }

        private static string TrimEndingDirectorySeparator(string directory)
        {
            return directory.TrimEnd('\\').TrimEnd('/');
        }
    }
}
