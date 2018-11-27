using System;
using System.IO;
using System.Threading.Tasks;

namespace TestsGeneratorLib.IO
{
    public class TestReader : ITestReader
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
