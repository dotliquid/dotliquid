using System.IO;

namespace DotLiquid
{
	public interface IRenderable
	{
		ReturnCode Render(Context context, TextWriter result);
	}
}