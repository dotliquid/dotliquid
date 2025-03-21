using System;
using System.Collections.Generic;

namespace DotLiquid.Tests.Helpers
{
    public class CustomRawTag : RawBlock
    {
        public override void Render(Context context, System.IO.TextWriter result)
        {
            result.WriteLine("I am a raw custom tag");
        }
    }

}
