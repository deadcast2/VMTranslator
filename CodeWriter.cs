using System.Collections.Generic;
using System.Linq;

namespace VMTranslator
{
    class CodeWriter
    {
        private Dictionary<string, string> _SegmentNameMap = new Dictionary<string, string>
        {
            { "local", "LCL" },
            { "argument", "ARG" },
            { "this", "THIS" },
            { "that", "THAT" }
        };

        private Dictionary<string, string> _TempMap = new Dictionary<string, string>
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

        private Dictionary<string, string> _PointerMap = new Dictionary<string, string>
        {
            { "0", "3" },
            { "1", "4" }
        };

        private int _ReturnCount = 0;

        private string _FileName;

        public void SetCurrentFileName(string name)
        {
            _FileName = name;
        }

        public string WriteBootstrap()
        {
            return
$@"// Bootstrap
@256
D=A
@SP
M=D
{WriteCall("sys.init", "0")}";
        }

        public string WriteComparators()
        {
            var equals =
@"// Equals
(EQ)
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
@"// Less than
(LT)
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
@"// Greater than
(GT)
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
$@" // Logical routines
@CODE
0;JMP
{equals}
{lessThan}
{greaterThan}
(CODE)";
        }

        public string WriteArithmetic(string operation, int lineNumber)
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
M=M+D
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
M=M-D
@SP
M=M+1";
                case "neg":
                    return
@"// Negate
@SP
M=M-1
A=M
M=-M
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
M=D&M
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
M=D|M
@SP
M=M+1";
                case "not":
                    return
@"// Not
@SP
M=M-1
A=M
M=!M
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

        public string WritePush(string segment, string value)
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
M=M+1";
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
@{_FileName}.{value}
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

        public string WritePop(string segment, string value)
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
@{_FileName}.{value}
M=D";
                default:
                    return null;
            }
        }

        public string WriteLabel(string label)
        {
            return $"({label})";
        }

        public string WriteIf(string label)
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

        public string WriteGoto(string label)
        {
            return
$@"// Goto {label}
@{label}
0;JMP";
        }

        public string WriteFunction(string name, string argumentCount)
        {
            var code = $"// Function {name} {argumentCount}\n({name})";

            int.TryParse(argumentCount, out int count);

            var pushes = Enumerable.Range(0, count).Select(i => $"\n{WritePush("constant", "0")}");

            return $"{code}{string.Join("", pushes)}";
        }

        public string WriteCall(string name, string argumentCount)
        {
            return
$@"// Call {name} {argumentCount}
// Push return address
@$ret.{_ReturnCount}
D=A
@SP
A=M
M=D
@SP
M=M+1
// Push local
@LCL
D=M
@SP
A=M
M=D
@SP
M=M+1
// Push argument
@ARG
D=M
@SP
A=M
M=D
@SP
M=M+1
// Push this
@THIS
D=M
@SP
A=M
M=D
@SP
M=M+1
// Push that
@THAT
D=M
@SP
A=M
M=D
@SP
M=M+1
// Reposition argument
@5
D=A
@SP
D=M-D
@{argumentCount}
D=D-A
@ARG
M=D
// Reposition local
@SP
D=M
@LCL
M=D
// Transfer control to callee
{WriteGoto(name)}
// Where to return once finished calling
($ret.{_ReturnCount++})";
        }

        public string WriteReturn()
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
A=M-D
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
A=M-1
D=M
@THAT
M=D
// Restore this
@2
D=A
@R13
A=M-D
D=M
@THIS
M=D
// Restore argument
@3
D=A
@R13
A=M-D
D=M
@ARG
M=D
// Restore local
@4
D=A
@R13
A=M-D
D=M
@LCL
M=D
// Goto return address
@R14
A=M
0;JMP";
        }
    }
}
