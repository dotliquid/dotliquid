using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotLiquid
{
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
	public abstract class Drop : ILiquidizable, IIndexable, IContextAware
	{
		private Dictionary<string, MethodInfo> _cachedMethods;
		private Dictionary<string, PropertyInfo> _cachedProperties;

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

		protected Drop()
		{
			// Cache all methods and properties of this object, but don't include those defined at or above the base Drop class.
			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
			_cachedMethods = GetType().GetMethods(bindingFlags).Where(mi => mi.GetParameters().Length == 0 && typeof(Drop).IsAssignableFrom(mi.DeclaringType.BaseType))
				.ToDictionary(mi => Template.NamingConvention.GetMemberName(mi.Name), Template.NamingConvention.StringComparer);
			_cachedProperties = GetType().GetProperties(bindingFlags).Where(pi => typeof(Drop).IsAssignableFrom(pi.DeclaringType.BaseType))
				.ToDictionary(pi => Template.NamingConvention.GetMemberName(pi.Name), Template.NamingConvention.StringComparer);
		}

		/// <summary>
		/// Catch all for the method
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		public virtual object BeforeMethod(string method)
		{
			return null;
		}

		/// <summary>
		/// Called by liquid to invoke a drop
		/// </summary>
		/// <param name="name"></param>
		public object InvokeDrop(object name)
		{
			string method = (string)name;

			if (_cachedMethods.ContainsKey(method))
				return _cachedMethods[method].Invoke(this, null);
			if (_cachedProperties.ContainsKey(method))
				return _cachedProperties[method].GetValue(this, null);
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
}