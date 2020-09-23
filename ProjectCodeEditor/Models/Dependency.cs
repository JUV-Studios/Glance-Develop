using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCodeEditor.Models
{
    public struct Dependency
    {
        public string DependencyName { get; set; }

        public Uri ProjectUri { get; set; }
    }
}
