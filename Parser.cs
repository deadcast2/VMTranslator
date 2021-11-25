using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VMTranslator
{
    class Parser
    {
        public enum CommandType { C_ARITHMETIC, C_PUSH, C_POP, C_LABEL, C_GOTO, C_IF, C_FUNCTION, C_RETURN, C_CALL, C_UNKNOWN }

        public Parser(string filepath)
        {
            try
            {
                var output = Process(File.ReadAllLines(filepath));

                File.WriteAllLines(Path.ChangeExtension(filepath, "asm"), output);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open file '{filepath}'. Error: {ex.Message}");
            }
        }

        private string[] Process(string[] lines)
        {
            var cleanedLines = RemoveWhiteSpaceAndComments(lines);
            var assemblyLines = new List<string>() { CodeWriter.WriteComparators() };

            for (var i = 0; i < cleanedLines.Length; i++)
            {
                var commandType = GetCommandType(cleanedLines[i], out var arg1, out var arg2);

                switch (commandType)
                {
                    case CommandType.C_ARITHMETIC:
                        assemblyLines.Add(CodeWriter.WriteArithmetic(arg1, i));
                        break;
                    case CommandType.C_PUSH:
                        assemblyLines.Add(CodeWriter.WritePush(arg1, arg2));
                        break;
                    case CommandType.C_POP:
                        assemblyLines.Add(CodeWriter.WritePop(arg1, arg2));
                        break;
                    case CommandType.C_LABEL:
                        assemblyLines.Add(CodeWriter.WriteLabel(arg1));
                        break;
                    case CommandType.C_IF:
                        assemblyLines.Add(CodeWriter.WriteIf(arg1));
                        break;
                    case CommandType.C_GOTO:
                        assemblyLines.Add(CodeWriter.WriteGoto(arg1));
                        break;
                    case CommandType.C_FUNCTION:
                        assemblyLines.Add(CodeWriter.WriteFunction(arg1, arg2));
                        break;
                    case CommandType.C_RETURN:
                        assemblyLines.Add(CodeWriter.WriteReturn());
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
