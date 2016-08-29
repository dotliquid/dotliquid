using DotLiquid.Exceptions;

namespace DotLiquid.FileSystems
{
    public class BlankFileSystem : IFileSystem
    {
        public string ReadTemplateFile(Context context, string templateName)
        {
            throw new FileSystemException(ResourceManager.BlankFileSystemDoesNotAllowIncludesException);
        }
    }
}
