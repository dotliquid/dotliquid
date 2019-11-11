using System.IO;
using System.Threading.Tasks;

namespace DotLiquid
{
    /// <summary>
    /// Object that can render itslef
    /// </summary>
    internal interface IRenderable
    {
        Task RenderAsync(Context context, TextWriter result);
    }
}
