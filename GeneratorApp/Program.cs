using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            TestsGeneratorLib.TestsGenerator generator = new TestsGeneratorLib.TestsGenerator(2, 1, 1, "outTests");

            generator.Generate(new List<string>()
            {
                "1.txt",
                "2.txt",
                "3.txt"
            }).Wait();

            Console.WriteLine("end...");
            Console.ReadKey();
        }
    }
}
