using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace TextEditor.IntelliSense
{
    public interface IFileIntelliSense : INotifyPropertyChanged, IDisposable
    {
        Task Parse(string fileText);

        Dictionary<string, IEnumerable<string>> SuggestionList { get; }
    }
}
