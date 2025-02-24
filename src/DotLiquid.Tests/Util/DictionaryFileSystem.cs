using System;
using System.Collections.Generic;
using DotLiquid.FileSystems;

namespace DotLiquid.Tests.Util
{
    internal class DictionaryFileSystem : IFileSystem
    {
        private readonly IDictionary<string, string> _templates;

        public DictionaryFileSystem(IDictionary<string, string> templates)
        {
            _templates = templates;
        }

        public string ReadTemplateFile(Context context, string templateName)
        {
            if (_templates.TryGetValue(templateName, out var template))
            {
                return template;
            }
            string templatePath = context[templateName] as string;
            if (templatePath != null && _templates.TryGetValue(templatePath, out template))
            {
                return template;
            }
            return templatePath;
        }
    }
}
