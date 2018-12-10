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
        private readonly int maxReadersCount;
        private readonly int maxGeneratorsCount;
        private readonly int maxWritersCount;
        private readonly ITestWriter writer;
        private readonly ITestReader reader;

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
            var readerBlock = new TransformBlock<string, string>(
                new Func<string, Task<string>>(reader.ReadAsync),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxReadersCount });

            var generatorBlock = new TransformBlock<string, List<GeneratedClass>>(
                new Func<string, Task<List<GeneratedClass>>>(GenerateTestFileAsync),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxGeneratorsCount });

            var writerBlock = new ActionBlock<List<GeneratedClass>>(
                new Action<List<GeneratedClass>>((x) => writer.WriteAsync(x).Wait()),
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

        private async Task<List<GeneratedClass>> GenerateTestFileAsync(string readedCode)
        {
            CompilationUnitSyntax compilationUnitSyntax = ParseCompilationUnit(readedCode);

            var classes = compilationUnitSyntax.DescendantNodes().OfType<ClassDeclarationSyntax>();
            List<UsingDirectiveSyntax> usings = new List<UsingDirectiveSyntax>
            {
                UsingDirective(QualifiedName(IdentifierName("Microsoft.VisualStudio"), IdentifierName("TestTools.UnitTesting")))
            };

            List<GeneratedClass> generatedTestNsClassesList = new List<GeneratedClass>();

            foreach (var cl in classes)
            {
                var methods = cl.DescendantNodes().OfType<MethodDeclarationSyntax>()
                    .Where(x => x.Modifiers.Any(t => t.ValueText == "public"));                

                string ns = (cl.Parent as NamespaceDeclarationSyntax)?.Name.ToString();
                if (ns == null)
                {
                    ns = "Global";
                }

                var methodsDeclarationList = TransformMethodsList(methods);
                var classDeclaration = PrepareClassDeclaration(cl.Identifier.ValueText + "Test", methodsDeclarationList, ns);
                
                generatedTestNsClassesList.Add(
                         new GeneratedClass(
                             cl.Identifier.ValueText + "Test",
                             PrepareResultDeclaration(ns, classDeclaration, usings).NormalizeWhitespace().ToFullString()
                         ));
            }

            return generatedTestNsClassesList;
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

        private MemberDeclarationSyntax PrepareClassDeclaration(string className, List<MemberDeclarationSyntax> methodsDeclarationList, string ns)
        {
            return ClassDeclaration(className)
                .WithAttributeLists(SingletonList(AttributeList(SingletonSeparatedList(Attribute(IdentifierName("TestClass"))))))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithMembers(List(methodsDeclarationList));
        }

        private CompilationUnitSyntax PrepareResultDeclaration(string ns, MemberDeclarationSyntax cl,
                                                                  List<UsingDirectiveSyntax> usings)
        {
            return CompilationUnit()
                    .WithUsings(List(usings))
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(
                            NamespaceDeclaration(QualifiedName(IdentifierName(ns), IdentifierName("Test")))
                    .WithMembers(SingletonList(cl))));

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
