using System.IO;
using System.Linq;

namespace DotLiquid.Tags
{
	/// <summary>
	/// Unless is a conditional just like 'if' but works on the inverse logic.
	/// 
	///  {% unless x &lt; 0 %} x is greater than zero {% end %}
	/// </summary>
	public class Unless : If
	{
		public override ReturnCode Render(Context context, TextWriter result)
		{
			return context.Stack(() =>
			{
				// First condition is interpreted backwards (if not)
				Condition block = Blocks.First();
				if (!block.Evaluate(context))
				{
					return RenderAll(block.Attachment, context, result);
				}

				// After the first condition unless works just like if
				foreach (Condition forEachBlock in Blocks.Skip(1))
					if (forEachBlock.Evaluate(context))
					{
						return RenderAll(forEachBlock.Attachment, context, result);
					}

				return ReturnCode.Return;
			});
		}
	}
}