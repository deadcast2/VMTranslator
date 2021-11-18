using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMTranslator
{
    static class CodeWriter
    {
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
                    return "@SP\nM=M-1\nA=M\nD=M\n@SP\nM=M-1\nA=M\nD=M+D\nM=D\n@SP\nM=M+1";
                case "sub":
                    return "@SP\nM=M-1\nA=M\nD=M\n@SP\nM=M-1\nA=M\nD=M-D\nM=D\n@SP\nM=M+1";
                case "neg":
                    return "@SP\nM=M-1\nA=M\nD=-M\nM=D\n@SP\nM=M+1";
                case "and":
                    return "@SP\nM=M-1\nA=M\nD=M\n@SP\nM=M-1\nA=M\nD=D&M\nM=D\n@SP\nM=M+1";
                case "or":
                    return "@SP\nM=M-1\nA=M\nD=M\n@SP\nM=M-1\nA=M\nD=D|M\nM=D\n@SP\nM=M+1";
                case "not":
                    return "@SP\nM=M-1\nA=M\nD=!M\nM=D\n@SP\nM=M+1";
                case "eq":
                case "lt":
                case "gt":
                    return 
$@"@SP
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
                    return $"@{value}\nD=A\n@SP\nA=M\nM=D\n@SP\nD=M\nM=D+1";
                default:
                    return null;
            }
        }
    }
}
