using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestsGeneratorLib.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.IO;

namespace TestsGeneratorLib.Test
{
    [TestClass]
    public class TestsGeneratorTests
    {
        string outputDirectory;
        string inputDirectory;
        ITestWriter writer;
        ITestReader reader;
        TestsGenerator generator;

        CompilationUnitSyntax generatedUnit;

        [ClassInitialize]
        public void Init()
        {
            outputDirectory = Path.Combine("..", "..", "TestDirectoryOutput");
            inputDirectory = Path.Combine("..", "..", "TestDirectoryInput");

            writer = new TestWriter(outputDirectory);
            reader = new TestReader();
            generator = new TestsGenerator(1, 1, 1, writer, reader);
            generator.Generate(Directory.GetFiles(inputDirectory).ToList()).Wait();

            generatedUnit = ParseCompilationUnit(File.ReadAllText(Path.Combine(outputDirectory, "TestClasses.cs")));
        }


        [TestMethod]
        public void TestFilesCount()
        {
            Assert.AreEqual(1, Directory.GetFiles(outputDirectory).Count());
        }

        [TestMethod]
        public void TestUsings()
        {
            string expectedUsing = "Microsoft.VisualStudio.TestTools.UnitTesting";
            Assert.IsTrue(generatedUnit.DescendantNodes().OfType<UsingDirectiveSyntax>().Any(x => x.Name.ToString() == expectedUsing),
                          " no such using : " + expectedUsing);
        }

        [TestMethod]
        public void TestCountOfNamespaces()
        {
            int actualCount = generatedUnit.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Count();
            Assert.AreEqual(2, actualCount);
        }

        [TestMethod]
        public void TestNamespacesName()
        {
            string expected1 = "TestClasses.Test";
            string expected2 = "Ns1.Test";

            Assert.IsTrue(generatedUnit.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Any(x => x.Name.ToString() == expected1),
                          " no such namespace : " + expected1);
            Assert.IsTrue(generatedUnit.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Any(x => x.Name.ToString() == expected2),
                          " no such namespace : " + expected2);
        }

        

        [TestMethod]
        public void TestClassesCount()
        {
            int actualCount = generatedUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().Count();
            Assert.AreEqual(3, actualCount, "classes count must be 3");
        }

        [TestMethod]
        public void TestClassesAttribute()
        {
            string attribute = "TestClass";

            Assert.IsTrue(generatedUnit.DescendantNodes().OfType<ClassDeclarationSyntax>()
                          .All(x => x.AttributeLists.Any(y => y.Attributes
                          .Any(z => z.ToString() == attribute))));
        }

        [TestMethod]
        public void TestMethodsAttribute()
        {
            string attribute = "TestMethod";

            Assert.IsTrue(generatedUnit.DescendantNodes().OfType<MethodDeclarationSyntax>()
                          .All(x => x.AttributeLists.Any(y => y.Attributes
                          .Any(z => z.ToString() == attribute))));
        }

        [TestMethod]
        public void TestRepeatedMethods()
        {
            string name = "Main2_Test";

            Assert.AreEqual(1, generatedUnit.DescendantNodes().OfType<MethodDeclarationSyntax>().Count(x => x.Identifier.ToString() == name));
        }


        [TestCleanup]
        public void CleanUp()
        {
            var files = Directory.GetFiles(outputDirectory);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}
