using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotLiquid.Tags
{
    /// <summary>
    /// Unless is a conditional just like 'if' but works on the inverse logic.
    ///
    ///  {% unless x &lt; 0 %} x is greater than zero {% end %}
    /// </summary>
    public class Unless : If
    {
        public override Task RenderAsync(Context context, TextWriter result)
        {
            return context.Stack(async () =>
            {
                // First condition is interpreted backwards (if not)
                Condition block = Blocks.First();
                if (!block.Evaluate(context, result.FormatProvider))
                {
                    await RenderAllAsync(block.Attachment, context, result).ConfigureAwait(false);
                    return;
                }

                // After the first condition unless works just like if
                foreach (Condition forEachBlock in Blocks.Skip(1))
                    if (forEachBlock.Evaluate(context, result.FormatProvider))
                    {
                        await RenderAllAsync(forEachBlock.Attachment, context, result).ConfigureAwait(false);
                        return;
                    }
            });
        }
    }
}
