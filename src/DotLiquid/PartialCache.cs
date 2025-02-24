using System.Collections.Generic;
using DotLiquid.FileSystems;

namespace DotLiquid
{
    internal static class PartialCache
    {
        private const string PartialCacheKey = "cached_partials";
        public static Template Load(string templateName, Context context)
        {
            var cachedPartials = context.Registers.Get<IDictionary<string, Template>>(PartialCacheKey);
            if (cachedPartials == null)
                context.Registers[PartialCacheKey] = cachedPartials = new Dictionary<string, Template>();
            if (cachedPartials.TryGetValue(templateName, out Template cached))
                return cached;
            Template partial = Fetch(templateName, context);
            cachedPartials[templateName] = partial;
            return partial;
        }

        public static Template Fetch(string templateName, Context context)
        {
            IFileSystem fileSystem = context.Registers["file_system"] as IFileSystem ?? Template.FileSystem;
            ITemplateFileSystem templateFileSystem = fileSystem as ITemplateFileSystem;
            Template partial = null;
            if (templateFileSystem != null)
            {
                partial = templateFileSystem.GetTemplate(context, templateName);
            }
            if (partial == null)
            {
                string source = fileSystem.ReadTemplateFile(context, templateName);
                partial = Template.Parse(source);
            }
            return partial;
        }
    }
}