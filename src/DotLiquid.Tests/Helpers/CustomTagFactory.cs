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
    }

}
