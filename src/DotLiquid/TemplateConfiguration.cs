using System;
using System.Collections.Generic;
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

        private readonly Dictionary<string, Type> _tags = new Dictionary<string, Type>();

        public IFileSystem FileSystem { get; set; }

        public TemplateConfiguration()
        {
            FileSystem = new BlankFileSystem();
            RegisterDefaultTags();
        }

        public void RegisterTag<T>(string name)
            where T : Tag, new()
        {
            _tags[name] = typeof(T);
        }

        public Type GetTagType(string name)
        {
            Type result;
            _tags.TryGetValue(name, out result);
            return result;
        }

        private void RegisterDefaultTags()
        {
            RegisterTag<Tags.Assign>("assign");
            RegisterTag<Tags.Block>("block");
            RegisterTag<Tags.Capture>("capture");
            RegisterTag<Tags.Case>("case");
            RegisterTag<Tags.Comment>("comment");
            RegisterTag<Tags.Cycle>("cycle");
            RegisterTag<Tags.Extends>("extends");
            RegisterTag<Tags.For>("for");
            RegisterTag<Tags.If>("if");
            RegisterTag<Tags.IfChanged>("ifchanged");
            RegisterTag<Tags.Include>("include");
            RegisterTag<Tags.Literal>("literal");
            RegisterTag<Tags.Unless>("unless");
            RegisterTag<Tags.Raw>("raw");

            RegisterTag<Tags.Html.TableRow>("tablerow");
        }
    }
}
