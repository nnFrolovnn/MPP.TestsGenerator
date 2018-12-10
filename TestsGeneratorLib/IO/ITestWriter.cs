using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGeneratorLib.IO
{
    public interface ITestWriter
    {
        Task WriteAsync(List<GeneratedClass> task);
    }
}
