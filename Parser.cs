using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VMTranslator
{
    class Parser
    {
        private CodeWriter _Writer = new CodeWriter();

        public enum CommandType { C_ARITHMETIC, C_PUSH, C_POP, C_LABEL, C_GOTO, C_IF, C_FUNCTION, C_RETURN, C_CALL, C_UNKNOWN }

        public Parser(string outputName, string[] filepaths)
        {
            var output = new List<string>();

            InsertInitialization(filepaths, output);

            foreach (var filepath in filepaths)
            {
                _Writer.SetCurrentFileName(Path.GetFileNameWithoutExtension(filepath));

                try
                {
                    output.AddRange(Process(File.ReadAllLines(filepath)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to open file '{filepath}'. Error: {ex.Message}");
                }
            }

            File.WriteAllLines($"{Path.ChangeExtension(outputName, "asm")}", output);
        }

        private void InsertInitialization(string[] filepaths, List<string> output)
        {
            if (filepaths.Length > 1)
            {
                output.Add(_Writer.WriteBootstrap());
            }

            output.Add(_Writer.WriteComparators());
        }

        private string[] Process(string[] lines)
        {
            var cleanedLines = RemoveWhiteSpaceAndComments(lines);
            var assemblyLines = new List<string>();

            for (var i = 0; i < cleanedLines.Length; i++)
            {
                var commandType = GetCommandType(cleanedLines[i], out var arg1, out var arg2);

                switch (commandType)
                {
                    case CommandType.C_ARITHMETIC:
                        assemblyLines.Add(_Writer.WriteArithmetic(arg1, i));
                        break;
                    case CommandType.C_PUSH:
                        assemblyLines.Add(_Writer.WritePush(arg1, arg2));
                        break;
                    case CommandType.C_POP:
                        assemblyLines.Add(_Writer.WritePop(arg1, arg2));
                        break;
                    case CommandType.C_LABEL:
                        assemblyLines.Add(_Writer.WriteLabel(arg1));
                        break;
                    case CommandType.C_IF:
                        assemblyLines.Add(_Writer.WriteIf(arg1));
                        break;
                    case CommandType.C_GOTO:
                        assemblyLines.Add(_Writer.WriteGoto(arg1));
                        break;
                    case CommandType.C_FUNCTION:
                        assemblyLines.Add(_Writer.WriteFunction(arg1, arg2));
                        break;
                    case CommandType.C_RETURN:
                        assemblyLines.Add(_Writer.WriteReturn());
                        break;
                    case CommandType.C_CALL:
                        assemblyLines.Add(_Writer.WriteCall(arg1, arg2));
                        break;
                }
            }

            return assemblyLines.ToArray();
        }

        private string[] RemoveWhiteSpaceAndComments(string[] lines)
        {
            var cleanedLines = new List<string>();

            foreach (var line in lines)
            {
                string modifiedLine = RemoveComments(line);

                if (string.IsNullOrWhiteSpace(modifiedLine))
                    continue;

                cleanedLines.Add(modifiedLine.Trim().ToLower());
            }

            return cleanedLines.ToArray();
        }

        private string RemoveComments(string line)
        {
            var commentStart = line.IndexOf("//");

            if (commentStart >= 0)
                line = line.Remove(commentStart);

            return line;
        }

        private CommandType GetCommandType(string line, out string arg1, out string arg2)
        {
            arg1 = arg2 = null;

            if (line.StartsWith("push"))
            {
                arg1 = GetArg1(line);
                arg2 = GetArg2(line);
                return CommandType.C_PUSH;
            }
            else if (line.StartsWith("pop"))
            {
                arg1 = GetArg1(line);
                arg2 = GetArg2(line);
                return CommandType.C_POP;
            }
            else if (IsArithmeticOperation(line))
            {
                arg1 = line;
                return CommandType.C_ARITHMETIC;
            }
            else if (line.StartsWith("label"))
            {
                arg1 = GetArg1(line);
                return CommandType.C_LABEL;
            }
            else if (line.StartsWith("if-goto"))
            {
                arg1 = GetArg1(line);
                return CommandType.C_IF;
            }
            else if (line.StartsWith("goto"))
            {
                arg1 = GetArg1(line);
                return CommandType.C_GOTO;
            }
            else if (line.StartsWith("function"))
            {
                arg1 = GetArg1(line);
                arg2 = GetArg2(line);
                return CommandType.C_FUNCTION;
            }
            else if (line.StartsWith("return"))
            {
                return CommandType.C_RETURN;
            }
            else if (line.StartsWith("call"))
            {
                arg1 = GetArg1(line);
                arg2 = GetArg2(line);
                return CommandType.C_CALL;
            }

            return CommandType.C_UNKNOWN;
        }

        private string GetArg1(string line)
        {
            var parts = line.Split(' ');

            if (parts.Length > 1) return parts[1];

            return null;
        }

        private string GetArg2(string line)
        {
            var parts = line.Split(' ');

            if (parts.Length > 2) return parts[2];

            return null;
        }

        private bool IsArithmeticOperation(string line)
        {
            return new[] { "add", "sub", "neg", "eq", "gt", "lt", "and", "or", "not" }
                .Any(op => line.StartsWith(op));
        }
    }
}
