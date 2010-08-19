using System.Text;

namespace DotLiquid
{
	internal interface IRenderable
	{
		void Render(Context context, StringBuilder result);
	}
}