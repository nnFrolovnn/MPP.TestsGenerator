using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGeneratorLib.IO
{
    class TestWriter : ITestWriter
    {
        private readonly string outputDir;

        public TestWriter(string outputDirectory)
        {
            if (outputDirectory != null)
            {
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }
                outputDir = outputDirectory;
            }
            else
            {
                throw new ArgumentNullException(nameof(outputDirectory));
            }
        }


        public async Task WriteAsync(GeneratedClass result)
        {
            string path = outputDir + "//" + result.ClassName + ".cs";

            using(StreamWriter writer = new StreamWriter(path))
            {
                await writer.WriteAsync(result.ToString());
            }
        }
    }
}
