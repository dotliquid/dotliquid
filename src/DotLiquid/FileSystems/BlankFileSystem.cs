using DotLiquid.Exceptions;

namespace DotLiquid.FileSystems
{
    public class BlankFileSystem : IFileSystem
    {
        /// <inheritdoc />
        public string ReadTemplateFile(Context context, string templateName)
        {
            throw new FileSystemException(Liquid.ResourceManager.GetString("BlankFileSystemDoesNotAllowIncludesException"));
        }
    }
}
