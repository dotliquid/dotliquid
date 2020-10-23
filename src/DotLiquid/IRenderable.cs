using System.IO;

namespace DotLiquid
{
    /// <summary>
    /// Object that can render itslef
    /// </summary>
    public interface IRenderable
    {
        void Render(Context context, TextWriter result);
    }

    /// <summary>
    /// Factory object that generates <see cref="IRenderable"/>s for templates
    /// </summary>
    public interface IRenderableFactory
    {
        IRenderable CreateVariable(string markup);
    }
}
