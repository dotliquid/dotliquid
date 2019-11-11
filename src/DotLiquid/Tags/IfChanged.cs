using System.IO;
using System.Threading.Tasks;

namespace DotLiquid.Tags
{
    public class IfChanged : DotLiquid.Block
    {
        public override Task RenderAsync(Context context, TextWriter result)
        {
            return context.Stack(async () =>
            {
                string tempString;
                using (TextWriter temp = new StringWriter(result.FormatProvider))
                {
                    await RenderAllAsync(NodeList, context, temp).ConfigureAwait(false);
                    tempString = temp.ToString();
                }

                if (tempString != (context.Registers["ifchanged"] as string))
                {
                    context.Registers["ifchanged"] = tempString;
                    await result.WriteAsync(tempString).ConfigureAwait(false);
                }
            });
        }
    }
}
