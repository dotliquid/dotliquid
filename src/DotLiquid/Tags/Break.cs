using System.IO;

namespace DotLiquid.Tags
{
    public class Break : Tag
    {
        public override ReturnCode Render(Context context, TextWriter result)
        {
            return ReturnCode.Break;
        }
    }
}
