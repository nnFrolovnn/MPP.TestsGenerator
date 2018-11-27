using System;
using System.Collections.Generic;
using System.IO;

namespace GeneratorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            TestsGeneratorLib.TestsGenerator generator = new TestsGeneratorLib.TestsGenerator(1, 1, 1, "outTests");

            generator.Generate(new List<string>()
            {
                Path.Combine("in","TestWriter.cs"),              
                Path.Combine("in","TestReader.cs"),
                Path.Combine("in","Program.cs"),
                Path.Combine("in","GeneratedClass.cs"),
                Path.Combine("in","TestsGenerator.cs")
            }).Wait();

            Console.WriteLine("end...");
            Console.ReadKey();
        }
    }
}
