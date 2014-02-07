using DotLiquid.FileSystems;

namespace DotLiquid
{
    public class TemplateConfiguration
    {
        private static TemplateConfiguration _global;

        public static TemplateConfiguration Global
        {
            get { return _global ?? (_global = new TemplateConfiguration()); }
        }

        public IFileSystem FileSystem { get; set; }

        public TemplateConfiguration()
        {
            this.FileSystem = new BlankFileSystem();
        }
    }
}
