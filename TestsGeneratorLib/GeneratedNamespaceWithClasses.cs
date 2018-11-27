using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGeneratorLib
{
    public class GeneratedNamespaceWithClasses
    {
        public string Name { get; }
        public string Content { get; }

        public GeneratedNamespaceWithClasses(string cName, string cContent)
        {
            Name = cName;
            Content = cContent;
        }
    }
}
