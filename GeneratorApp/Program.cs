using System;
using System.Collections.Generic;
using System.IO;
using TestsGeneratorLib.IO;

namespace GeneratorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            TestWriterSingle writer = new TestWriterSingle("outTests");
            TestReader reader = new TestReader();
            TestsGeneratorLib.TestsGenerator generator = new TestsGeneratorLib.TestsGenerator(1, 1, 1, writer, reader);

            Console.WriteLine("Started...");

            generator.Generate(new List<string>()
            {
                Path.Combine("in","TestWriter.cs"),              
                Path.Combine("in","TestReader.cs"),
                Path.Combine("in","Program.cs"),
                Path.Combine("in","GeneratedClass.cs"),
                Path.Combine("in","TestsGenerator.cs"),
                Path.Combine("in","TestClasses.cs")
            }).Wait();

            Console.WriteLine("Completed...");
            Console.ReadKey();
        }
    }
}
