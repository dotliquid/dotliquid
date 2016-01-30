using System.IO;
using System.Text.RegularExpressions;

namespace DotLiquid.Tags
{
	public class Comment : DotLiquid.Block
	{
		private static Regex _commentRegex = new Regex(Liquid.CommentShorthand, RegexOptions.Compiled);
		public static string FromShortHand(string @string)
		{
			if (@string == null)
				return @string;

			var match = _commentRegex.Match(@string);
			return match.Success ? string.Format(@"{{% comment %}}{0}{{% endcomment %}}", match.Groups[1].Value) : @string;
		}

		public override ReturnCode Render(Context context, TextWriter result)
		{
			return ReturnCode.Return;
		}
	}
}