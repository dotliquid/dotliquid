using System.IO;
using DotLiquid.Exceptions;

namespace DotLiquid.Tags
{
    public class Break : Tag
    {
        public override void Render(Context context, TextWriter result)
        {
            throw new BreakInterrupt();
        }
    }
}
