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

            var generatorBlock = new TransformBlock<Task<string>, Task<List<GeneratedClass>>>(
                new Func<Task<string>, Task<List<GeneratedClass>>>(GenerateTestFileAsync),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxGeneratorsCount });

            var writerBlock = new ActionBlock<Task<List<GeneratedClass>>>(
                new Action<Task<List<GeneratedClass>>>((x) => writer.WriteAsync(x).Wait()),
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

        private async Task<List<GeneratedClass>> GenerateTestFileAsync(Task<string> readSourceFile)
        {
            string readedCode = await readSourceFile;
            CompilationUnitSyntax compilationUnitSyntax = ParseCompilationUnit(readedCode);

            List<GeneratedClass> generatedTestClassesList = new List<GeneratedClass>();

            var classes = compilationUnitSyntax.DescendantNodes().OfType<ClassDeclarationSyntax>();
            var usings = List<UsingDirectiveSyntax>();
            usings.Add(UsingDirective(QualifiedName(IdentifierName("Microsoft.VisualStudio"), IdentifierName("TestTools.UnitTesting"))));

            foreach (var cl in classes)
            {
                var methods = cl.DescendantNodes().OfType<MethodDeclarationSyntax>()
                    .Where(x => x.Modifiers.Any(t => t.ValueText == "public"));

                string ns = (cl.Parent as NamespaceDeclarationSyntax)?.Name.ToString();
                if (ns == null)
                {
                    ns = "Global";
                }

                SyntaxList<MemberDeclarationSyntax> methodsDeclarationList = new SyntaxList<MemberDeclarationSyntax>();
                foreach (var meth in methods)
                {
                    string name = meth.Identifier.ToString();
                    if (methodsDeclarationList.Count != 0 &&
                        methodsDeclarationList.First(x => (x as MethodDeclarationSyntax)?.Identifier == meth.Identifier) != null)
                    {
                        int i = 1;
                        while (methodsDeclarationList.First(x => (x as MethodDeclarationSyntax)?.Identifier.ToString() == name + "_" + i) != null)
                        {
                            i++;
                        }
                        name += "_" + i;
                    }
                    methodsDeclarationList.Add(PrepareMethodDeclaration(name));
                }

                CompilationUnitSyntax unit = PrepareUnit(usings, cl.Identifier.ValueText + "Test", ns, methodsDeclarationList);

                generatedTestClassesList.Add(
                    new GeneratedClass(
                        cl.Identifier.ValueText + "Test",
                        unit.NormalizeWhitespace().ToFullString())
                    );
            }


            return generatedTestClassesList;
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

        private CompilationUnitSyntax PrepareUnit(SyntaxList<UsingDirectiveSyntax> usings, string className, 
                                                  string ns, SyntaxList<MemberDeclarationSyntax> methodsDeclarationList)
        {
            return CompilationUnit()
                    .WithUsings(usings)
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(
                            NamespaceDeclaration(QualifiedName(IdentifierName(ns), IdentifierName("Test")))
                        .WithMembers(SingletonList<MemberDeclarationSyntax>(ClassDeclaration(className)
                            .WithAttributeLists(SingletonList(AttributeList(
                                    SingletonSeparatedList(Attribute(IdentifierName("TestClass"))))))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                                .WithMembers(methodsDeclarationList)))));
        }
    }
}
