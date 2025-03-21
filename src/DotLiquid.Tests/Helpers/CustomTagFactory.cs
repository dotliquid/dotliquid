using System;
using System.Collections.Generic;

namespace DotLiquid.Tests.Helpers
{
    public class CustomTagFactory : ITagFactory
    {
        public string TagName
        {
            get { return "custom"; }
        }

        public Tag Create()
        {
            return new CustomTag();
        }

        public class CustomTag : Tag
        {
            public override void Render(Context context, System.IO.TextWriter result)
            {
                result.WriteLine("I am a custom tag");
            }
        }
    }

}
