using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGeneratorLib
{
    class NamespaceInfo
    {
        public List<ClassInfo> Classes { get; set; }
        public string Name { get; }

        public NamespaceInfo(string name)
        {
            Name = name;
            Classes = new List<ClassInfo>();
        }
        public NamespaceInfo(string name, ClassInfo info)
        {
            Name = name;
            Classes = new List<ClassInfo>() { info };
        }
    }
}
