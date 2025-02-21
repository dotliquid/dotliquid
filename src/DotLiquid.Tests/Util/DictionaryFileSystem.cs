using System;
using System.Collections.Generic;
using DotLiquid.FileSystems;

namespace DotLiquid.Tests.Util
{
    internal class DictionaryFileSystem : IFileSystem
    {
        public Dictionary<string, string> Templates;

        public DictionaryFileSystem(Dictionary<string, string> templates)
        {
            Templates = templates;
        }

        public string ReadTemplateFile(Context context, string templateName)
        {
            string templatePath = (string)context[templateName];

            if (templatePath != null && Templates.TryGetValue(templatePath, out var template))
                return template;

            return templatePath;
        }
    }
}
