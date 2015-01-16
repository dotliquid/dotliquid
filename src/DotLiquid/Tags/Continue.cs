using System.IO;

namespace DotLiquid.Tags
{
    public class Continue : Tag
    {
        public override ReturnCode Render(Context context, TextWriter result)
        {
            return ReturnCode.Continue;
        }
    }
}