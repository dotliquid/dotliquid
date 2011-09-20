using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotLiquid.NamingConventions;

namespace DotLiquid
{
    /// <summary>
    /// Configurable typing metadata collection
    /// </summary>
    internal class TypeResolution
    {
        public Dictionary<string, MethodInfo> _cachedMethods;
        public Dictionary<string, PropertyInfo> _cachedProperties;

        public TypeResolution(Type t) : this(t, true, false) { }
        public TypeResolution(Type t, bool dropDerived, bool declaredOnly)
        {
            // Cache all methods and properties of this object, but don't include those defined at or above the base Drop class.
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            if (declaredOnly) { bindingFlags |= BindingFlags.DeclaredOnly; }
            _cachedMethods = t.GetMethods(bindingFlags).Where(mi => mi.GetParameters().Length == 0 && (!dropDerived || typeof(Drop).IsAssignableFrom(mi.DeclaringType.BaseType)))
                .ToDictionary(mi => Template.NamingConvention.GetMemberName(mi.Name), Template.NamingConvention.StringComparer);
            _cachedProperties = t.GetProperties(bindingFlags).Where(pi => !dropDerived || typeof(Drop).IsAssignableFrom(pi.DeclaringType.BaseType))
                .ToDictionary(pi => Template.NamingConvention.GetMemberName(pi.Name), Template.NamingConvention.StringComparer);
        }
    }

	/// <summary>
	/// A drop in liquid is a class which allows you to to export DOM like things to liquid
	/// Methods of drops are callable.
	/// The main use for liquid drops is the implement lazy loaded objects.
	/// If you would like to make data available to the web designers which you don't want loaded unless needed then
	/// a drop is a great way to do that
	///
	/// Example:
	///
	/// class ProductDrop &lt; Liquid::Drop
	/// def top_sales
	/// Shop.current.products.find(:all, :order => 'sales', :limit => 10 )
	/// end
	/// end
	///
	/// tmpl = Liquid::Template.parse( ' {% for product in product.top_sales %} {{ product.name }} {%endfor%} ' )
	/// tmpl.render('product' => ProductDrop.new ) # will invoke top_sales query.
	///
	/// Your drop can either implement the methods sans any parameters or implement the before_method(name) method which is a
	/// catch all
	/// </summary>
	public abstract class DropBase : ILiquidizable, IIndexable, IContextAware
	{
		internal TypeResolution _resolution;

		public Context Context { get; set; }

		/// <summary>
		/// Just an alias for InvokeDrop - but the presence of the indexer
		/// means that Liquid will access Drop objects as though they are
		/// dictionaries or hashes.
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		public object this[object method]
		{
			get { return InvokeDrop(method); }
		}

		/// <summary>
		/// Catch all for the method
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		public virtual object BeforeMethod(string method)
		{
			// Quite a common (and easy) mistake is to use C#-style property names,
			// without realising that the default naming convention is Ruby-style.
			// To try to help with this, we check if the given name *would* match,
			// if we were using Ruby-style names.
			if (Template.NamingConvention is RubyNamingConvention)
			{
				string rubyMethod = Template.NamingConvention.GetMemberName(method);

				MethodInfo mi;
				PropertyInfo pi;
				if (_resolution._cachedMethods.TryGetValue(rubyMethod, out mi)
					|| _resolution._cachedProperties.TryGetValue(rubyMethod, out pi))
				{
					return string.Format(Liquid.ResourceManager.GetString("DropWrongNamingConventionMessage"), rubyMethod);
				}
			}
			return null;
		}

        /// <summary>
        /// Called by liquid to invoke a drop
        /// </summary>
        /// <param name="name"></param>
        public abstract object InvokeDrop(object name);

		public virtual bool ContainsKey(object name)
		{
			return true;
		}

		public virtual object ToLiquid()
		{
			return this;
		}
	}

    public abstract class Drop : DropBase
    {
        [ThreadStatic] private Util.WeakTable<Type, TypeResolution> _cache = new Util.WeakTable<Type, TypeResolution>(32);

        protected Drop() 
        {
            Type t = GetType();
            if (!_cache.TryGetValue(t, out _resolution))
                _cache[t] = _resolution = new TypeResolution(t);
        }

        /// <summary>
        /// Called by liquid to invoke a drop
        /// </summary>
        /// <param name="name"></param>
        public override object InvokeDrop(object name)
        {
            string method = (string)name;

            MethodInfo mi;
            if (_resolution._cachedMethods.TryGetValue(method, out mi))
                return mi.Invoke(this, null);
            PropertyInfo pi;
            if (_resolution._cachedProperties.TryGetValue(method, out pi))
                return pi.GetValue(this, null);
            return BeforeMethod(method);
        }
    }

    /// <summary>
    /// Proxy for types not derived from DropBase
    /// </summary>
    public class DropProxy : DropBase {
        [ThreadStatic] private Util.WeakTable<Type, TypeResolution> _cache = new Util.WeakTable<Type, TypeResolution>(32);

        private readonly object proxiedObject;
        public DropProxy(object obj)
        {
            proxiedObject = obj;
            Type t = obj.GetType();
            if (!_cache.TryGetValue(t, out _resolution))
                _cache[t] = _resolution = new TypeResolution(t, false, true);
        }

        public override object InvokeDrop(object name)
        {
            string method = (string)name;

            MethodInfo mi;
            if (_resolution._cachedMethods.TryGetValue(method, out mi))
                return mi.Invoke(proxiedObject, null);
            PropertyInfo pi;
            if (_resolution._cachedProperties.TryGetValue(method, out pi))
                return pi.GetValue(proxiedObject, null);
            return BeforeMethod(method);
        }
    }
}