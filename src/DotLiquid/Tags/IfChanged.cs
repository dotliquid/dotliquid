using System.IO;

namespace DotLiquid.Tags
{
	public class IfChanged : DotLiquid.Block
	{
		public override ReturnCode Render(Context context, TextWriter result)
		{
			return context.Stack(() =>
			{
				string tempString;
				using (TextWriter temp = new StringWriter())
				{
					var retCode = RenderAll(NodeList, context, temp);
					if (retCode != ReturnCode.Return)
						return retCode;
					tempString = temp.ToString();
				}

				if (tempString != (context.Registers["ifchanged"] as string))
				{
					context.Registers["ifchanged"] = tempString;
					result.Write(tempString);
				}

				return ReturnCode.Return;
			});
		}
	}
}