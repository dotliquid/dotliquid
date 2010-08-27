using System.Collections.Generic;
using System.Text;

namespace DotLiquid.Tags
{
	public class IfChanged : DotLiquid.Block
	{
		public override void Render(Context context, StringBuilder result)
		{
			context.Stack(() =>
			{
				StringBuilder temp = new StringBuilder();
				RenderAll(NodeList, context, temp);
				string tempString = temp.ToString();

				if (tempString != (context.Registers["ifchanged"] as string))
				{
					context.Registers["ifchanged"] = tempString;
					result.Append(tempString);
				}
			});
		}
	}
}