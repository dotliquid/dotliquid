using DotLiquid.Exceptions;

namespace DotLiquid.FileSystems
{
    public class BlankFileSystem : IFileSystem
    {
        public object ReadTemplateFile(Context context, string templateName)
        {
            throw new FileSystemException(Liquid.ResourceManager.GetString("BlankFileSystemDoesNotAllowIncludesException"));
        }
    }
}
