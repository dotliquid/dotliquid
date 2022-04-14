using System.IO;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;

namespace DotLiquid.FileSystems
{
    /// <summary>
    /// This implements a file system which retrieves template files from embedded resources in .NET assemblies.
    ///
    /// Its behavior is the same as with the Local File System, except this uses namespaces and embedded resources
    /// instead of directories and files.
    ///
    /// Example:
    ///
    /// var fileSystem = new EmbeddedFileSystem("My.Base.Namespace");
    ///
    /// fileSystem.FullPath("mypartial") # => "My.Base.Namespace._mypartial.liquid"
    /// fileSystem.FullPath("dir/mypartial") # => "My.Base.Namespace.dir._mypartial.liquid"
    /// </summary>
    public class EmbeddedFileSystem : IFileSystem
    {
        protected System.Reflection.Assembly Assembly { get; private set; }

        public string Root { get; private set; }

        /// <inheritdoc />
        public EmbeddedFileSystem(System.Reflection.Assembly assembly, string root)
        {
            Assembly = assembly;
            Root = root;
        }

        /// <inheritdoc />
        public string ReadTemplateFile(Context context, string templateName)
        {
            var templatePath = (string)context[templateName];
            var fullPath = FullPath(templatePath);

            var stream = Assembly.GetManifestResourceStream(fullPath);
            if (stream == null)
                throw new FileSystemException(
                    Liquid.ResourceManager.GetString("LocalFileSystemTemplateNotFoundException"), templatePath);

            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public string FullPath(string templatePath)
        {
            if (templatePath == null || !Liquid.LimitRelativePathRegex.IsMatch(templatePath))
                throw new FileSystemException(
                    Liquid.ResourceManager.GetString("LocalFileSystemIllegalTemplateNameException"), templatePath);

            var directorySeparators = Liquid.DirectorySeparatorsRegex;
            var basePath = directorySeparators.IsMatch(templatePath)
                ? directorySeparators.Replace(input: Path.Combine(Root, Path.GetDirectoryName(templatePath)), replacement: ".")
                : Root;

            var fileName = string.Format("_{0}.liquid", Path.GetFileName(templatePath));

            var fullPath = $"{basePath}.{fileName}";

            return fullPath;
        }
    }
}
