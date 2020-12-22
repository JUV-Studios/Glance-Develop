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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TextEditor.Lexer
{
    public class Tokenizer
    {
        public Tokenizer(IGrammer grammer)
        {
            var grammerRules = new List<GrammerRule>(grammer.Rules);
            if (grammer == null) throw new ArgumentNullException("grammer");
            if (grammer.Keywords == null) throw new ArgumentException("Grammer Keywords must not be null");
            else grammerRules.Insert(0, new(ScopeName.Keyword, WordRegex(grammer.Keywords)));
            if (grammer.Builtins == null) throw new ArgumentException("Grammer Builtins must not be null");
            else grammerRules.Insert(0, new GrammerRule(ScopeName.Predefined, WordRegex(grammer.Builtins)));
            // grammerRules.Insert(0, new GrammerRule("Whitespace", new Regex("^\\s")));
            GrammerRules = grammerRules;
        }

        public IEnumerable<GrammerRule> GrammerRules { get; private set; }

        static Regex WordRegex(IEnumerable<string> words)
        {
            return new Regex("^((" + string.Join(")|(", words.Where(s => !string.IsNullOrWhiteSpace(s))) + "))\\b");
        }

        internal IEnumerator<Token> Tokenize(string script)
        {
            var builder = new StringBuilder(script);
            int i = 0;
            int length = script.Length;
            Match match;
            bool found;
            string str = script;

            while (i < length)
            {
                found = false;

                foreach (var rule in GrammerRules)
                {
                    match = rule.Pattern.Match(str);
                    if (match.Success)
                    {
                        if (match.Length == 0) throw new InvalidOperationException("Regex Pattern matches string of length zero");
                        yield return new Token(i, match.Length, rule.Captures[0]);
                        i += match.Length;
                        builder.Remove(0, match.Length);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    yield return new Token(i, 1, ScopeName.PlainText);
                    i += 1;
                    builder.Remove(0, 1);
                }

                str = builder.ToString();
            }
        }
    }
}