using DotLiquid.FileSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotLiquid.Tests.Helpers
{
    public class TestTemplateFileSystem : ITemplateFileSystem
    {
        private IDictionary<string, Template> _templateCache = new Dictionary<string, Template>();
        private IFileSystem _baseFileSystem = null;
        private int _cacheHitTimes;
        public int CacheHitTimes { get { return _cacheHitTimes; } }

        public TestTemplateFileSystem(IFileSystem baseFileSystem)
        {
            _baseFileSystem = baseFileSystem;
        }

        public string ReadTemplateFile(Context context, string templateName)
        {
            return _baseFileSystem.ReadTemplateFile(context, templateName);
        }

        public Template GetTemplate(Context context, string templateName)
        {
            Template template;
            if (_templateCache.TryGetValue(templateName, out template))
            {
                ++_cacheHitTimes;
                return template;
            }
            var result = ReadTemplateFile(context, templateName);
            template = Template.Parse(result);
            _templateCache[templateName] = template;
            return template;
        }
    }
}
