using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGeneratorLib.IO
{
    public class TestWriterSingle : ITestWriter
    {
        private readonly string outputDir;

        public TestWriterSingle(string outputDirectory)
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

        public async Task WriteAsync(List<GeneratedClass> result)
        {          
            foreach (GeneratedClass cl in result)
            {
                string path = Path.Combine(outputDir, cl.Name + ".cs");
                using (FileStream writer = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    byte[] buff = Encoding.ASCII.GetBytes(cl.Content);
                    await writer.WriteAsync(buff, 0, buff.Length);
                }
            }
        }
    }
}

