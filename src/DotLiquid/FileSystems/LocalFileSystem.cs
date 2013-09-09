using System.IO;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;

namespace DotLiquid.FileSystems
{
	/// <summary>
	/// This implements an abstract file system which retrieves template files named in a manner similar to Rails partials,
	/// ie. with the template name prefixed with an underscore. The extension ".liquid" is also added.
	/// 
	/// For security reasons, template paths are only allowed to contain letters, numbers, and underscore.
	/// 
	/// Example:
	/// 
	/// file_system = Liquid::LocalFileSystem.new("/some/path")
	/// 
	/// file_system.full_path("mypartial") # => "/some/path/_mypartial.liquid"
	/// file_system.full_path("dir/mypartial") # => "/some/path/dir/_mypartial.liquid"
	/// </summary>
	public class LocalFileSystem : IFileSystem
	{
		public string Root { get; set; }

		public LocalFileSystem(string root)
		{
			Root = root;
		}

		public string ReadTemplateFile(Context context, string templateName)
		{
			string templatePath = (string) context[templateName];
			string fullPath = FullPath(templatePath);
			if (!File.Exists(fullPath))
				throw new FileSystemException(Liquid.ResourceManager.GetString("LocalFileSystemTemplateNotFoundException"), templatePath);
			return File.ReadAllText(fullPath);
		}

		public string FullPath(string templatePath)
		{
			if (templatePath == null || !Regex.IsMatch(templatePath, @"^[^.\/][a-zA-Z0-9_\/]+$"))
				throw new FileSystemException(Liquid.ResourceManager.GetString("LocalFileSystemIllegalTemplateNameException"), templatePath);

			string fullPath = templatePath.Contains("/")
				? Path.Combine(Path.Combine(Root, Path.GetDirectoryName(templatePath)), string.Format("_{0}.liquid", Path.GetFileName(templatePath)))
				: Path.Combine(Root, string.Format("_{0}.liquid", templatePath));

			string escapedPath = Root.Replace(@"\", @"\\").Replace("(", @"\(").Replace(")", @"\)");
			if (!Regex.IsMatch(Path.GetFullPath(fullPath), string.Format("^{0}", escapedPath)))
				throw new FileSystemException(Liquid.ResourceManager.GetString("LocalFileSystemIllegalTemplatePathException"), Path.GetFullPath(fullPath));

			return fullPath;
		}
	}
}