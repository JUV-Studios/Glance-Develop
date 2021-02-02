// Copyright (c) Adnan Umer. All rights reserved. Follow me @aztnan
// Email: aztnan@outlook.com
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using ColorCode.Common;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TextEditor.Lexer;
using Windows.ApplicationModel;

namespace TextEditor.Languages
{
    public class PythonSyntaxLanguage : SyntaxLanguage
    {
        public PythonSyntaxLanguage()
        {
            Name = "Python";
            Id = ".py";
            Rules = new GrammerRule[]
            {
                new GrammerRule(ScopeName.Keyword, Tokenizer.WordRegex(Keywords)),
                new GrammerRule(ScopeName.BuiltinFunction, Tokenizer.WordRegex(Builtins)),
                new GrammerRule(ScopeName.Comment, new Regex("^(#.*)", RegexOptions.Singleline)), // Comment
                new GrammerRule(ScopeName.Operator, new Regex("^[\\+\\-\\*/%&|\\^~<>!]")), // Single Char Operator
                new GrammerRule(ScopeName.Operator, new Regex("^((==)|(!=)|(<=)|(>=)|(<>)|(<<)|(>>)|(//)|(\\*\\*))")), // Double Char Operator
                new GrammerRule(ScopeName.Delimiter, new Regex("^[\\(\\)\\[\\]\\{\\}@,:`=;\\.]")), // Single Delimiter
                new GrammerRule(ScopeName.Delimiter, new Regex("^((\\+=)|(\\-=)|(\\*=)|(%=)|(/=)|(&=)|(\\|=)|(\\^=))")), // Double Char Operator
                new GrammerRule(ScopeName.Delimiter, new Regex("^((//=)|(>>=)|(<<=)|(\\*\\*=))")), // Triple Delimiter
                new GrammerRule(ScopeName.TypeVariable, new Regex("^[_A-Za-z][_A-Za-z0-9]*")), // Identifier
                new GrammerRule(ScopeName.String, new Regex("^((\"\"\"(.*)\"\"\")|('''(.)*'''))", RegexOptions.IgnoreCase | RegexOptions.Multiline)),
                new GrammerRule(ScopeName.String, new Regex("^((@'(?:[^']|'')*'|'(?:\\.|[^\\']|)*('|\\b))|(@\"(?:[^\"]|\"\")*\"|\"(?:\\.|[^\\\"])*(\"|\\b)))",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline)), // String Marker
            };

            IndentationProvider = new PythonIndentationProvider();
        }

        private static readonly string PythonFolderPath = Path.Combine(Package.Current.InstalledPath, "Assets", "Languages", "Python");

        private static readonly IEnumerable<string> Builtins = File.ReadAllLines(Path.Combine(PythonFolderPath, "Builtins"));

        private static readonly IEnumerable<string> Keywords = File.ReadAllLines(Path.Combine(PythonFolderPath, "Keywords"));
    }
}