using DotLiquid.FileSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotLiquid.Tests.Util
{
    public class CountingFileSystem : IFileSystem
    {
        public int Count { get; private set; }

        public string ReadTemplateFile(Context context, string templateName)
        {
            Count++;
            return "from CountingFileSystem";
        }
    }
}
