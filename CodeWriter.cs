﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace VMTranslator
{
    static class CodeWriter
    {
        private static Dictionary<string, string> _SegmentNameMap = new Dictionary<string, string>
        {
            { "local", "LCL" },
            { "argument", "ARG" },
            { "this", "THIS" },
            { "that", "THAT" }
        };

        private static Dictionary<string, string> _TempMap = new Dictionary<string, string>
        {
            { "0", "5" },
            { "1", "6" },
            { "2", "7" },
            { "3", "8" },
            { "4", "9" },
            { "5", "10" },
            { "6", "11" },
            { "7", "12" }
        };

        private static Dictionary<string, string> _PointerMap = new Dictionary<string, string>
        {
            { "0", "3" },
            { "1", "4" }
        };

        public static string WriteComparators()
        {
            var equals = 
@"(EQ)
@SP
A=M
D=M
@NOT_EQ
D;JNE
@SP
A=M
M=-1
@R13
A=M
0;JMP
(NOT_EQ)
@SP
A=M
M=0
@R13
A=M
0;JMP";

            var lessThan =
@"(LT)
@SP
A=M
D=M
@NOT_LT
D;JGE
@SP
A=M
M=-1
@R13
A=M
0;JMP
(NOT_LT)
@SP
A=M
M=0
@R13
A=M
0;JMP";

            var greaterThan =
@"(GT)
@SP
A=M
D=M
@NOT_GT
D;JLE
@SP
A=M
M=-1
@R13
A=M
0;JMP
(NOT_GT)
@SP
A=M
M=0
@R13
A=M
0;JMP";

            return 
$@"@CODE
0;JMP
{equals}
{lessThan}
{greaterThan}
(CODE)";
        }

        public static string WriteArithmetic(string operation, int lineNumber)
        {
            switch (operation)
            {
                case "add":
                    return
@"// Add
@SP
M=M-1
A=M
D=M
@SP
M=M-1
A=M
D=M+D
M=D
@SP
M=M+1";
                case "sub":
                    return 
@"// Subtract
@SP
M=M-1
A=M
D=M
@SP
M=M-1
A=M
D=M-D
M=D
@SP
M=M+1";
                case "neg":
                    return 
@"// Negate
@SP
M=M-1
A=M
D=-M
M=D
@SP
M=M+1";
                case "and":
                    return 
@"// And
@SP
M=M-1
A=M
D=M
@SP
M=M-1
A=M
D=D&M
M=D
@SP
M=M+1";
                case "or":
                    return 
@"// Or
@SP
M=M-1
A=M
D=M
@SP
M=M-1
A=M
D=D|M
M=D
@SP
M=M+1";
                case "not":
                    return 
@"// Not
@SP
M=M-1
A=M
D=!M
M=D
@SP
M=M+1";
                case "eq":
                case "lt":
                case "gt":
                    return 
$@"// {operation}
@SP
M=M-1
A=M
D=M
@SP
M=M-1
A=M
D=M-D
@SP
A=M
M=D
@COMPARE_{lineNumber}
D=A
@R13
M=D
@{operation.ToUpper()}
0;JMP
(COMPARE_{lineNumber})
@SP
M=M+1";
                default:
                    return null;
            }
        }

        public static string WritePush(string segment, string value)
        {
            switch (segment)
            {
                case "constant":
                    return 
$@"// Push {segment} {value}
@{value}
D=A
@SP
A=M
M=D
@SP
D=M
M=D+1";
                case "local":
                case "this":
                case "that":
                case "argument":
                    return
$@"// Push {segment} {value}
@{value}
D=A
@{_SegmentNameMap[segment]}
D=M+D
A=D
D=M
@SP
A=M
M=D
@SP
M=M+1";
                case "temp":
                    return
$@"// Push {segment} {value}
@{_TempMap[value]}
D=M
@SP
A=M
M=D
@SP
M=M+1";
                case "pointer":
                    return
$@"// Push {segment} {value}
@{_PointerMap[value]}
D=M
@SP
A=M
M=D
@SP
M=M+1";
                case "static":
                    return
$@"// Push {segment} {value}
@{int.Parse(value) + 16}
D=M
@SP
A=M
M=D
@SP
M=M+1";
                default:
                    return null;
            }
        }

        public static string WritePop(string segment, string value)
        {
            switch (segment)
            {
                case "local":
                case "argument":
                case "this":
                case "that":
                    return
$@"// Pop {segment} {value}
@SP
M=M-1
A=M
D=M
@R13
M=D
@{value}
D=A
@{_SegmentNameMap[segment]}
D=M+D
@R14
M=D
@R13
D=M
@R14
A=M
M=D";
                case "temp":
                    return
$@"// Pop {segment} {value}
@SP
M=M-1
A=M
D=M
@{_TempMap[value]}
M=D";
                case "pointer":
                    return
$@"// Pop {segment} {value}
@SP
M=M-1
A=M
D=M
@{_PointerMap[value]}
M=D";
                case "static":
                    return
$@"// Pop {segment} {value}
@SP
M=M-1
A=M
D=M
@{int.Parse(value) + 16}
M=D";
                default:
                    return null;
            }
        }

        public static string WriteLabel(string label)
        {
            return $"({label})";
        }

        public static string WriteIf(string label)
        {
            return 
$@"// If goto {label}
@SP
M=M-1
A=M
D=M
@{label}
D;JNE";
        }

        public static string WriteGoto(string label)
        {
            return
$@"// Goto {label}
@{label}
0;JMP";
        }

        public static string WriteFunction(string name, string argumentCount)
        {
            var code = $"// Function {name} {argumentCount}";

            int.TryParse(argumentCount, out int count);

            var pushes = Enumerable.Range(0, count).Select(i => WritePush("constant", "0"));

            return $"{code}\n{string.Join("\n", pushes)}";
        }

        public static string WriteReturn()
        {
            return
$@"// Return
// Store frame location
@LCL
D=M
@R13
M=D
// Store return address
@5
D=A
@R13
D=M-D
A=D
D=M
@R14
M=D
// Store arg and restore stack pointer
@SP
M=M-1
A=M
D=M
@ARG
A=M
M=D
@ARG
D=M
@SP
M=D+1
// Restore that
@R13
D=M-1
A=D
D=M
@THAT
M=D
// Restore this
@2
D=A
@R13
D=M-D
A=D
D=M
@THIS
M=D
// Restore argument
@3
D=A
@R13
D=M-D
A=D
D=M
@ARG
M=D
// Restore local
@4
D=A
@R13
D=M-D
A=D
D=M
@LCL
M=D";
        }
    }
}
