using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestsGeneratorLib.IO;

namespace TestsGeneratorLib.Test
{
    [TestClass]
    public class TestsGeneratorTests
    {
        const string outputDirectory = "../../TestDirectoryOutput";
        const string inputDirectory = "../../TestDirectoryInput";
        ITestWriter writer;
        ITestReader reader;
        TestsGenerator generator;

        [TestInitialize]
        public void Init()
        {
            writer = new TestWriter(outputDirectory);
            reader = new TestReader();
            generator = new TestsGenerator(1, 1, 1, writer, reader);
        }


        [TestMethod]
        public void TestMethod1()
        {
        }

        [TestMethod]
        public void TestMethod2()
        {
        }

        [TestMethod]
        public void TestMethod3()
        {
        }

        [TestMethod]
        public void TestMethod4()
        {
        }

        [TestMethod]
        public void TestMethod5()
        {
        }

        [TestMethod]
        public void TestMethod6()
        {
        }

    }
}
