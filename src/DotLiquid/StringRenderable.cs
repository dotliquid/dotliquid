using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotLiquid
{
    public class StringRenderable : IRenderable
    {
        private readonly string _body;
        public StringRenderable(string body)
        {
            _body = body;
        }

        public ReturnCode Render(Context context, System.IO.TextWriter result)
        {
            if (!string.IsNullOrEmpty(_body))
                result.Write(_body);

            return ReturnCode.Return;
        }
    }
}
