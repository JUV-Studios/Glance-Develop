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
    public class PythonGrammer : IGrammer
    {
        private static readonly string PythonFolderPath = Path.Combine(Package.Current.InstalledPath, "Assets", "Languages", "Python");

        public IEnumerable<GrammerRule> Rules { get; } = new GrammerRule[]
        {
            new GrammerRule(ScopeName.Comment, new Regex("^(#.*)", RegexOptions.Compiled)),  // Comment
                //new GrammerRule(ScopeName.Operator, new Regex("^(and|or|not|is)\\b")), // Word Operator
            new GrammerRule(ScopeName.Operator, new Regex("^[\\+\\-\\*/%&|\\^~<>!]", RegexOptions.Compiled)), // Single Char Operator
            new GrammerRule(ScopeName.Operator, new Regex("^((==)|(!=)|(<=)|(>=)|(<>)|(<<)|(>>)|(//)|(\\*\\*))", RegexOptions.Compiled)), // Double Char Operator
            new GrammerRule(ScopeName.Delimiter, new Regex("^[\\(\\)\\[\\]\\{\\}@,:`=;\\.]", RegexOptions.Compiled)), // Single Delimiter
            new GrammerRule(ScopeName.Delimiter, new Regex("^((\\+=)|(\\-=)|(\\*=)|(%=)|(/=)|(&=)|(\\|=)|(\\^=))", RegexOptions.Compiled)), // Double Char Operator
            new GrammerRule(ScopeName.Delimiter, new Regex("^((//=)|(>>=)|(<<=)|(\\*\\*=))", RegexOptions.Compiled)), // Triple Delimiter
            new GrammerRule(ScopeName.TypeVariable, new Regex("^[_A-Za-z][_A-Za-z0-9]*", RegexOptions.Compiled)), // Identifier
            new GrammerRule(ScopeName.String, new Regex("^((\"\"\"(.*)\"\"\")|('''(.)*'''))", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled)),
            new GrammerRule(ScopeName.String, new Regex("^((@'(?:[^']|'')*'|'(?:\\.|[^\\']|)*('|\\b))|(@\"(?:[^\"]|\"\")*\"|\"(?:\\.|[^\\\"])*(\"|\\b)))", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled)), // String Marker
        };
            
        public IEnumerable<string> Builtins { get; } = File.ReadAllLines(Path.Combine(PythonFolderPath, "Builtins"));

        public IEnumerable<string> Keywords { get; } = File.ReadAllLines(Path.Combine(PythonFolderPath, "Keywords"));
    }
}