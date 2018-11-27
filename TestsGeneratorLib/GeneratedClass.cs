using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGeneratorLib
{
    class GeneratedClass
    {
        public string Name { get; }
        public string Content { get; }

        public GeneratedClass(string cName, string cContent)
        {
            Name = cName;
            Content = cContent;
        }
    }
}
