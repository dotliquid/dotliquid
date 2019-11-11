using DotLiquid.Exceptions;
using System.Threading.Tasks;

namespace DotLiquid.FileSystems
{
    public class BlankFileSystem : IFileSystem
    {
        public Task<string> ReadTemplateFileAsync(Context context, string templateName)
        {
            throw new FileSystemException(Liquid.ResourceManager.GetString("BlankFileSystemDoesNotAllowIncludesException"));
        }
    }
}
