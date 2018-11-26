using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGeneratorLib.IO
{
    public class TestWriter : ITestWriter
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

        public async Task WriteAsync(Task<List<GeneratedClass>> task)
        {
            var result = await task;
            string path = outputDir + "//" + result[0].Name + ".cs";
            
            using (FileStream writer = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None))
            {
                foreach (GeneratedClass cl in result)
                {
                    byte[] buff = Encoding.ASCII.GetBytes(cl.Content);
                    await writer.WriteAsync(buff, 0, buff.Length);
                }
            }
        }
    }
}
