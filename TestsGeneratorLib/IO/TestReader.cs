using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestsGeneratorLib.IO
{
    public class TestReader : ITestReader
    {
        public async Task<string> ReadAsync(string path)
        {
            if (path != null)
            {
                return RR(path);

            }
            else
            {
                throw new ArgumentNullException(nameof(path));
            }
        }

        public string RR(string path)
        {
            Thread.Sleep(10000);
            using (StreamReader reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
