using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
	/// <summary>
	/// Raw
	/// Raw outputs text as is, usefull if your template contains Liquid syntax.
	/// 
	/// {% raw %}{% if user = 'tobi' %}hi{% endif %}{% endraw %}
	/// </summary>
	public class Raw : DotLiquid.Block
	{
		protected override void Parse(List<string> tokens)
		{
            NodeList = NodeList ?? new List<IRenderable>();
			NodeList.Clear();

			string token;
			while ((token = tokens.Shift()) != null)
			{
				Match fullTokenMatch = FullToken.Match(token);
				if (fullTokenMatch.Success && BlockDelimiter == fullTokenMatch.Groups[1].Value)
				{
					EndTag();
					return;
				}

				NodeList.Add(new StringRenderable(token));
			}

			AssertMissingDelimitation();
		}
	}
}