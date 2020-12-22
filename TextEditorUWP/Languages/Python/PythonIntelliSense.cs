using ColorCode.Common;
using IronPython.Compiler.Ast;
using Microsoft.Toolkit.HighPerformance.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TextEditor.IntelliSense;

namespace TextEditor.Languages.Python
{
    public sealed class PythonIntelliSense : IFileIntelliSense
    {
        public bool Parsing = false;

        private string FileText;

        private readonly List<FunctionDefinition> _FunctionList = new();

        private readonly Dictionary<string, IEnumerable<string>> _SuggestionList = new();

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            _FunctionList.Clear();
            _SuggestionList.Clear();
        }

        public async Task Parse(string fileText)
        {
            bool needsParse = true;
            if (FileText == null) needsParse = true;
            else if (FileText.TrimEnd() != fileText.TrimEnd()) needsParse = true;
            else needsParse = false;
            if (!Parsing && needsParse)
            {
                Parsing = true;
                FileText = fileText;
                await Task.Run(() =>
                {
                    Dispose();
                    RetriveFunctionList();
                    List<string> functionList = new();
                    Parallel.ForEach(_FunctionList, function =>
                    {
                        functionList.Add(function.Name);
                    });

                    functionList.Sort();
                    SuggestionList["My functions"] = functionList;
                });
                Parsing = false;
                PropertyChanged?.Invoke(this, new("SuggestionList[FunctionList]"));
            }
        }

        public Dictionary<string, IEnumerable<string>> SuggestionList => _SuggestionList;

        private void RetriveFunctionList()
        {
            Parallel.ForEach(FileText.Split('\r', StringSplitOptions.RemoveEmptyEntries), line =>
            {
                if (line.StartsWith("def") && line.EndsWith(":"))
                {
                    var info = line.Remove(0, 3).TrimEnd(':').Trim();
                    var startBracketIndex = info.IndexOf('(');
                    var endBracketIndex = info.IndexOf(')');
                    if (startBracketIndex != -1 && endBracketIndex != -1 && endBracketIndex > startBracketIndex)
                    {
                        var name = info.Substring(0, startBracketIndex);
                        List<Parameter> funcParams = new();
                        var paramStr = info.Substring(startBracketIndex).Trim('(', ')');
                        var paramListRaw = info.Tokenize(',');
                        var identifierRegex = LanguageProvider.CodeLanguages[".py"].Value.Grammer.Rules.Where(item => item.Captures[0] == ScopeName.TypeVariable).First().Pattern;
                        while (paramListRaw.MoveNext())
                        {
                            var paramName = new string(paramListRaw.Current.ToArray());
                            if (identifierRegex.IsMatch(paramName)) funcParams.Add(new Parameter(paramName, ParameterKind.Normal));
                        }

                        if (identifierRegex.IsMatch(name)) _FunctionList.Add(new FunctionDefinition(name, funcParams.ToArray()));
                    }
                }
            });
        }
    }
}
