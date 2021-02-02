using System;
using System.Threading.Tasks;

namespace ProjectCodeEditor.Models
{
    internal interface IClosable : IDisposable
    {
        Task<bool> CloseAsync(bool showDialog = true);
    }
}
