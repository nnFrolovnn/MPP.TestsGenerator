using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestsGeneratorLib.IO;

namespace TestsGeneratorLib
{
    public class TestsGenerator
    {
        private int maxReadersCount;
        private int maxGeneratorsCount;
        private int maxWritersCount;
        private ITestWriter writer;
        private ITestReader reader;

        public TestsGenerator(int maxReadersCount, int maxGeneratorsCount, int maxWritersCount)
        {
            this.maxGeneratorsCount = maxGeneratorsCount;
            this.maxReadersCount = maxReadersCount;
            this.maxWritersCount = maxWritersCount;
            writer = new TestWriter("DefaultOUT");
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



        }
    }
}
