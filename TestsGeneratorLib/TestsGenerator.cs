using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TestsGeneratorLib.IO;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsGeneratorLib
{
    public class TestsGenerator
    {
        private int maxReadersCount;
        private int maxGeneratorsCount;
        private int maxWritersCount;
        private ITestWriter writer;
        private ITestReader reader;

        public TestsGenerator(int maxReadersCount, int maxGeneratorsCount, int maxWritersCount, string outputDir)
        {
            this.maxGeneratorsCount = maxGeneratorsCount;
            this.maxReadersCount = maxReadersCount;
            this.maxWritersCount = maxWritersCount;
            writer = new TestWriter(outputDir);
            reader = new TestReader();
        }

        public TestsGenerator(int maxReadersCount, int maxGeneratorsCount,
                              int maxWritersCount, ITestWriter writer, ITestReader reader)
        {
            this.maxGeneratorsCount = maxGeneratorsCount;
            this.maxReadersCount = maxReadersCount;
            this.maxWritersCount = maxWritersCount;
            this.writer = writer;
            this.reader = reader;
        }

        public Task Generate(List<string> files)
        {
            var readerBlock = new TransformBlock<string, Task<string>>(
                new Func<string, Task<string>>(reader.ReadAsync),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxReadersCount });

            var generatorBlock = new TransformBlock<Task<string>, Task<List<GeneratedNamespaceWithClasses>>>(
                new Func<Task<string>, Task<List<GeneratedNamespaceWithClasses>>>(GenerateTestFileAsync),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxGeneratorsCount });

            var writerBlock = new ActionBlock<Task<List<GeneratedNamespaceWithClasses>>>(
                new Action<Task<List<GeneratedNamespaceWithClasses>>>((x) => writer.WriteAsync(x).Wait()),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxWritersCount });

            var options = new DataflowLinkOptions { PropagateCompletion = true };

            readerBlock.LinkTo(generatorBlock, options);
            generatorBlock.LinkTo(writerBlock, options);

            foreach (var file in files)
            {
                readerBlock.Post(file);
            }

            readerBlock.Complete();

            return writerBlock.Completion;
        }

        private async Task<List<GeneratedNamespaceWithClasses>> GenerateTestFileAsync(Task<string> readSourceFile)
        {
            string readedCode = await readSourceFile;
            CompilationUnitSyntax compilationUnitSyntax = ParseCompilationUnit(readedCode);
            var classes = compilationUnitSyntax.DescendantNodes().OfType<ClassDeclarationSyntax>();
            List<UsingDirectiveSyntax> usings = new List<UsingDirectiveSyntax>
            {
                UsingDirective(QualifiedName(IdentifierName("Microsoft.VisualStudio"), IdentifierName("TestTools.UnitTesting")))
            };

            List<NamespaceInfo> nsInfoList = new List<NamespaceInfo>();
            foreach (var cl in classes)
            {
                var methods = cl.DescendantNodes().OfType<MethodDeclarationSyntax>()
                    .Where(x => x.Modifiers.Any(t => t.ValueText == "public"));

                var methodsDeclarationList = TransformMethodsList(methods);

                string ns = (cl.Parent as NamespaceDeclarationSyntax)?.Name.ToString();
                if (ns == null)
                {
                    ns = "Global";
                }            

                //save created class with methods in namespace
                AddToList(ref nsInfoList, new ClassInfo(cl.Identifier.ValueText + "Test", methodsDeclarationList), ns);
            }
       
            return TransformData(nsInfoList, usings);
        }

        private MethodDeclarationSyntax PrepareMethodDeclaration(string methodName)
        {
            return MethodDeclaration(
                            PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier(methodName + "_" + "Test"))
                                .WithAttributeLists(SingletonList(AttributeList(SingletonSeparatedList(
                                    Attribute(IdentifierName("TestMethod"))))))
                                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                                .WithBody(Block(ExpressionStatement(InvocationExpression(
                                     MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                     IdentifierName("Assert"), IdentifierName("Fail")))
                                .WithArgumentList(ArgumentList(SingletonSeparatedList(
                                    Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                    Literal("test")))))))));

        }

        private MemberDeclarationSyntax PrepareClassDeclaration(string className, List<MemberDeclarationSyntax> methodsDeclarationList)
        {
            return ClassDeclaration(className)
                .WithAttributeLists(SingletonList(AttributeList(SingletonSeparatedList(Attribute(IdentifierName("TestClass"))))))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithMembers(List(methodsDeclarationList));
        }

        private CompilationUnitSyntax PrepareNamespaceDeclaration(string ns, List<MemberDeclarationSyntax> classes,
                                                                  List<UsingDirectiveSyntax> usings = null)
        {
            return (usings != null) ?
                CompilationUnit()
                    .WithUsings(List(usings))
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(
                            NamespaceDeclaration(QualifiedName(IdentifierName(ns), IdentifierName("Test")))
                    .WithMembers(List(classes))))
               : CompilationUnit()
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(
                            NamespaceDeclaration(QualifiedName(IdentifierName(ns), IdentifierName("Test")))
                    .WithMembers(List(classes))));
        }

        private List<GeneratedNamespaceWithClasses> TransformData(List<NamespaceInfo> nsInfoList, List<UsingDirectiveSyntax> usings)
        {
            List<GeneratedNamespaceWithClasses> generatedTestNsClassesList = new List<GeneratedNamespaceWithClasses>();

            List<MemberDeclarationSyntax> classList;
            foreach (var nsItem in nsInfoList)
            {
                classList = new List<MemberDeclarationSyntax>();

                foreach (var clItem in nsItem.Classes)
                {
                    classList.Add(PrepareClassDeclaration(clItem.ClassName, clItem.MethodsDeclarationList));
                }

                if (generatedTestNsClassesList.Count == 0)
                {
                    generatedTestNsClassesList.Add(
                        new GeneratedNamespaceWithClasses(
                            nsItem.Name,
                            PrepareNamespaceDeclaration(nsItem.Name, classList, usings).NormalizeWhitespace().ToFullString() + '\n'
                        ));
                }
                else
                {
                    generatedTestNsClassesList.Add(
                        new GeneratedNamespaceWithClasses(
                            nsItem.Name,
                            PrepareNamespaceDeclaration(nsItem.Name, classList).NormalizeWhitespace().ToFullString() + '\n'
                        ));
                }
            }

            return generatedTestNsClassesList;
        }

        private void AddToList(ref List<NamespaceInfo> nsInfoList, ClassInfo clInfo, string ns)
        {
            if (nsInfoList.Count != 0)
            {
                NamespaceInfo namespaceInfo = nsInfoList.Find(x => x.Name == ns);
                if (namespaceInfo != null)
                {
                    namespaceInfo.Classes.Add(clInfo);
                }
                else
                {
                    nsInfoList.Add(new NamespaceInfo(ns, clInfo));
                }
            }
            else
            {
                nsInfoList.Add(new NamespaceInfo(ns, clInfo));
            }
        }

        private List<MemberDeclarationSyntax> TransformMethodsList(IEnumerable<MethodDeclarationSyntax> methods)
        {
            List<MemberDeclarationSyntax> methodsDeclarationList = new List<MemberDeclarationSyntax>();
            foreach (var meth in methods)
            {
                string name = meth.Identifier.ToString();

                if (methodsDeclarationList.Count != 0 && methodsDeclarationList.Any(x =>
                            (x as MethodDeclarationSyntax)?.Identifier.ToString() == meth.Identifier.ToString() + "_Test"))
                {
                    int i = 1;
                    while (methodsDeclarationList.Any(x => (x as MethodDeclarationSyntax)?.Identifier.ToString()
                                                            == meth.Identifier.ToString() + "_" + i + "_Test"))
                    {
                        i++;
                    }
                    name += "_" + i;
                }

                methodsDeclarationList.Add(PrepareMethodDeclaration(name));
            }

            return methodsDeclarationList;
        }
    }
}
