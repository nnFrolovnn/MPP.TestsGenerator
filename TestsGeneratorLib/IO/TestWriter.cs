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
            string path = outputDir + "//" + result.Name + ".cs";
            
            using (FileStream writer = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None))
            {
                byte[] buff = Encoding.ASCII.GetBytes(result.Content);
                await writer.WriteAsync(buff, 0, buff.Length);
            }
        }
    }
}
