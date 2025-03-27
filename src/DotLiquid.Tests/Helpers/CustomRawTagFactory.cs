using System;
using System.Collections.Generic;

namespace DotLiquid.Tests.Helpers
{
    public class CustomRawTagFactory : ITagFactory
    {
        public string TagName
        {
            get { return "customraw"; }
        }

        public Tag Create()
        {
            return new CustomRawTag();
        }
    }

}
