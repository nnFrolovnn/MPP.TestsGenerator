using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGeneratorLib
{
    class ClassInfo
    {
        public List<MemberDeclarationSyntax> MethodsDeclarationList { get; }
        public string ClassName { get; }

        public ClassInfo (string className, List<MemberDeclarationSyntax> list)
        {
            MethodsDeclarationList = list;
            ClassName = className;
        }
    }
}
