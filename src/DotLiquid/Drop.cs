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
		public Dictionary<string, MethodInfo> CachedMethods { get; private set; }
		public Dictionary<string, PropertyInfo> CachedProperties { get; private set; }

    	public TypeResolution(Type type, Func<MemberInfo, bool> filterMemberCallback)
		{
            // Cache all methods and properties of this object, but don't include those 
			// defined at or above the base Drop class.
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
    		CachedMethods = GetMemberDictionary(
				type.GetMethods(bindingFlags).Where(mi => mi.GetParameters().Length == 0),
				mi => filterMemberCallback(mi));
            CachedProperties = GetMemberDictionary(type.GetProperties(bindingFlags),
				mi => filterMemberCallback(mi));
        }

		private Dictionary<string, T> GetMemberDictionary<T>(IEnumerable<T> members,
			Func<T, bool> filterMemberCallback)
			where T : MemberInfo
		{
			return members.Where(filterMemberCallback).ToDictionary(mi =>
				Template.NamingConvention.GetMemberName(mi.Name),
				Template.NamingConvention.StringComparer);
		}
    }

	internal static class TypeResolutionCache
	{
		[ThreadStatic] private static Util.WeakTable<Type, TypeResolution> _cache;

		public static Util.WeakTable<Type, TypeResolution> Instance
		{
			get { return _cache ?? (_cache = new Util.WeakTable<Type, TypeResolution>(32)); }
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
		private TypeResolution _resolution;

		internal TypeResolution TypeResolution
		{
			get
			{
				Type dropType = GetObject().GetType();
				if (!TypeResolutionCache.Instance.TryGetValue(dropType, out _resolution))
					TypeResolutionCache.Instance[dropType] = _resolution = CreateTypeResolution(dropType);
				return _resolution;
			}
		}

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

		internal abstract object GetObject();
		internal abstract TypeResolution CreateTypeResolution(Type type);

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
				if (TypeResolution.CachedMethods.TryGetValue(rubyMethod, out mi)
					|| TypeResolution.CachedProperties.TryGetValue(rubyMethod, out pi))
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
		public object InvokeDrop(object name)
		{
			string method = (string)name;

			MethodInfo mi;
			if (TypeResolution.CachedMethods.TryGetValue(method, out mi))
				return mi.Invoke(GetObject(), null);
			PropertyInfo pi;
			if (TypeResolution.CachedProperties.TryGetValue(method, out pi))
				return pi.GetValue(GetObject(), null);
			return BeforeMethod(method);
		}

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
		internal override object GetObject()
		{
			return this;
		}

		internal override TypeResolution CreateTypeResolution(Type type)
		{
			return new TypeResolution(type, mi => typeof(Drop).IsAssignableFrom(mi.DeclaringType.BaseType));
		}
    }

    /// <summary>
    /// Proxy for types not derived from DropBase
    /// </summary>
    public class DropProxy : DropBase, IValueTypeConvertible
	{
        private readonly object _proxiedObject;
    	private readonly string[] _allowedMembers;
        private readonly Func<object, object> _value;

    	/// <summary>
    	/// Create a new DropProxy object
    	/// </summary>
    	/// <param name="obj">The object to create a proxy for</param>
    	/// <param name="allowedMembers">An array of property and method names that are allowed to be called on the object.</param>
    	public DropProxy(object obj, string[] allowedMembers)
        {
            _proxiedObject = obj;
    		_allowedMembers = allowedMembers;
        }

        public DropProxy(object obj, string[] allowedMembers, Func<object, object> value)
        {
            _proxiedObject = obj;
            _allowedMembers = allowedMembers;
            _value = value;
        }

        public virtual object ConvertToValueType()
        {
            if(_value == null)
                return null;

            return _value(_proxiedObject);
        }

		internal override object GetObject()
		{
			return _proxiedObject;
		}

		internal override TypeResolution CreateTypeResolution(Type type)
		{
			return new TypeResolution(type, mi => _allowedMembers.Contains(mi.Name));
		}
    }
}