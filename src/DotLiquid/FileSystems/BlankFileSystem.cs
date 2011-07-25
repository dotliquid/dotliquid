using DotLiquid.Exceptions;

namespace DotLiquid.FileSystems
{
	public class BlankFileSystem : IFileSystem
	{
		public string ReadTemplateFile(string templateName, Context context)
		{
			throw new FileSystemException(Liquid.ResourceManager.GetString("BlankFileSystemDoesNotAllowIncludesException"));
		}
	}
}