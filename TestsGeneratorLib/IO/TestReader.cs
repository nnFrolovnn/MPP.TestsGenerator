using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGeneratorLib.IO
{
    class TestReader : ITestReader
    {
        public async Task<string> ReadAsync(string path)
        {
            if (path != null)
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(path));
            }
        }
    }
}
