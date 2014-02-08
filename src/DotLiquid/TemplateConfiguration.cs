using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly Dictionary<Type, Func<object, object>> _safeTypeTransformers =
            new Dictionary<Type, Func<object, object>>();

        private readonly Dictionary<Type, Func<object, object>> _valueTypeTransformers =
            new Dictionary<Type, Func<object, object>>();

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

        /// <summary>
        /// Registers a simple type. DotLiquid will wrap the object in a <see cref="DropProxy"/> object.
        /// </summary>
        /// <param name="type">The type to register</param>
        /// <param name="allowedMembers">An array of property and method names that are allowed to be called on the object.</param>
        public void RegisterSafeType(Type type, string[] allowedMembers)
        {
            RegisterSafeType(type, x => new DropProxy(x, allowedMembers));
        }

        /// <summary>
        /// Registers a simple type. DotLiquid will wrap the object in a <see cref="DropProxy"/> object.
        /// </summary>
        /// <param name="type">The type to register</param>
        /// <param name="allowedMembers">An array of property and method names that are allowed to be called on the object.</param>
        public void RegisterSafeType(Type type, string[] allowedMembers, Func<object, object> func)
        {
            RegisterSafeType(type, x => new DropProxy(x, allowedMembers, func));
        }

        /// <summary>
        /// Registers a simple type using the specified transformer.
        /// </summary>
        /// <param name="type">The type to register</param>
        /// <param name="func">Function that converts the specified type into a Liquid Drop-compatible object (eg, implements ILiquidizable)</param>
        public void RegisterSafeType(Type type, Func<object, object> func)
        {
            _safeTypeTransformers[type] = func;
        }

        public Func<object, object> GetSafeTypeTransformer(Type type)
        {
            // Check for concrete types
            if (_safeTypeTransformers.ContainsKey(type))
                return _safeTypeTransformers[type];

            // Check for interfaces
            foreach (var interfaceType in _safeTypeTransformers.Where(x => x.Key.IsInterface))
            {
                if (type.GetInterfaces().Contains(interfaceType.Key))
                    return interfaceType.Value;
            }

            return null;
        }

        /// <summary>
        /// Registers a simple value type transformer.  Used for rendering a variable to the output stream
        /// </summary>
        /// <param name="type">The type to register</param>
        /// <param name="func">Function that converts the specified type into a Liquid Drop-compatible object (eg, implements ILiquidizable)</param>
        public void RegisterValueTypeTransformer(Type type, Func<object, object> func)
        {
            _valueTypeTransformers[type] = func;
        }

        public Func<object, object> GetValueTypeTransformer(Type type)
        {
            // Check for concrete types
            if (_valueTypeTransformers.ContainsKey(type))
                return _valueTypeTransformers[type];

            // Check for interfaces
            foreach (var interfaceType in _valueTypeTransformers.Where(x => x.Key.IsInterface))
            {
                if (type.GetInterfaces().Contains(interfaceType.Key))
                    return interfaceType.Value;
            }

            return null;
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
