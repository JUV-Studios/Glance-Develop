using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCodeEditor.Helpers
{
    interface ISuspendable
    {
        bool Suspended { get; }

        Task SuspendAsync();

        Task ResumeAsync();
    }
}
