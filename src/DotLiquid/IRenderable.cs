using System.IO;

namespace DotLiquid
{
	internal interface IRenderable
	{
		void Render(Context context, TextWriter result);
	}
}