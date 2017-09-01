using System.IO;

namespace DotLiquid
{
    /// <summary>
    /// Object that can render itslef
    /// </summary>
    internal interface IRenderable
    {
        void Render(Context context, TextWriter result);
    }
}
