using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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
                new Action<Task<List<GeneratedClass>>>((x)=>writer.WriteAsync(x).Wait()),
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
            var s = await readSourceFile;
            return new List<GeneratedClass>() { new GeneratedClass(s + "1", "int q"), new GeneratedClass(s + "2", "int t") };
        }

    }
}
