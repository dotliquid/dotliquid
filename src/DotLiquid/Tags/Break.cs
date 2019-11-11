using System.IO;
using System.Threading.Tasks;
using DotLiquid.Exceptions;

namespace DotLiquid.Tags
{
    public class Break : Tag
    {
        public async override Task RenderAsync(Context context, TextWriter result)
        {
            throw new BreakInterrupt();
        }
    }
}
