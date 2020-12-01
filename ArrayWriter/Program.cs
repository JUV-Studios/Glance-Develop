using System.IO;

namespace ArrayWriter
{
    class Program
    {
        static void Main(string[] args)
        {
            var Builtins = new string[]
            {
                "ArithmeticError", "AssertionError", "AttributeError", "BaseException",
                "BufferError", "BytesWarning", "DeprecationWarning", "EOFError", "Ellipsis",
                "EnvironmentError", "Exception", "False", "FloatingPointError", "FutureWarning",
                "GeneratorExit", "IOError", "ImportError", "ImportWarning", "IndentationError",
                "IndexError", "KeyError", "KeyboardInterrupt", "LookupError", "MemoryError",
                "NameError", "None", "NotImplemented", "NotImplementedError", "OSError",
                "OverflowError", "PendingDeprecationWarning", "ReferenceError", "RuntimeError",
                "RuntimeWarning", "StandardError", "StopIteration", "SyntaxError",
                "SyntaxWarning", "SystemError", "SystemExit", "TabError", "True", "TypeError",
                "UnboundLocalError", "UnicodeDecodeError", "UnicodeEncodeError", "UnicodeError",
                "UnicodeTranslateError", "UnicodeWarning", "UserWarning", "ValueError", "Warning",
                "WindowsError", "ZeroDivisionError", "_", "__debug__", "__doc__", "__import__",
                "__name__", "__package__", "abs", "all", "any", "apply", "basestring", "bin",
                "bool", "buffer", "bytearray", "bytes", "callable", "chr", "classmethod",
                "cmp", "coerce", "complex", "copyright", "credits", "delattr",
                "dict", "divmod", "enumerate", "filter", "float",
                "format", "frozenset", "getattr", "hasattr",
                "hash", "hex", "help", "id", "int", "intern", "isinstance", "issubclass",
                "iter", "len", "license", "list", "long", "map", "max", "memoryview",
                "min", "next", "object", "oct", "ord", "pow", "print", "property",
                "range", "raw_input", "reduce", "repr", "reversed",
                "round", "set", "setattr", "slice", "sorted", "staticmethod", "str", "sum",
                "super", "tuple", "type", "unichr", "unicode", "xrange", "zip"
            };

            File.WriteAllLines(nameof(Builtins), Builtins);
        }
    }
}
