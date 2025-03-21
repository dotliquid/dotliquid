using System;
using System.Collections.Generic;

namespace DotLiquid.Tests.Helpers
{
    public class CustomTag : Tag
    {
        public override void Render(Context context, System.IO.TextWriter result)
        {
            result.WriteLine("I am a custom tag");
        }
    }
}
