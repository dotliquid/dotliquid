using System.IO;

namespace DotLiquid
{
	internal interface IRenderable
	{
		ReturnCode Render(Context context, TextWriter result);
	}
}