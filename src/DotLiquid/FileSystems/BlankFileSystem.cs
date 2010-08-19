using DotLiquid.Exceptions;

namespace DotLiquid.FileSystems
{
	public class BlankFileSystem : IFileSystem
	{
		public string ReadTemplateFile(string templatePath)
		{
			throw new FileSystemException("This liquid context does not allow includes.");
		}
	}
}