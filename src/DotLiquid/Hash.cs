using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DotLiquid
{
    /// <summary>
    /// Represents a collection of keys and values and is a DotLiquid safe type
    /// </summary>
    public class Hash : IDictionary<string, object>, IDictionary
    {
        #region Static fields

        private static System.Collections.Concurrent.ConcurrentDictionary<string, Action<object, Hash>> mapperCache = new System.Collections.Concurrent.ConcurrentDictionary<string, Action<object, Hash>>();

        #endregion

        #region Fields
        private readonly Func<Hash, string, object> _lambda;
        private readonly Dictionary<string, object> _nestedDictionary;
        private readonly object _defaultValue;

        #endregion

        #region Static construction methods

        /// <summary>
        /// Initializes a new instance of the <see cref="DotLiquid.Hash">Hash</see> class and populates it with the contents of an anonymous object
        /// </summary>
        /// <param name="anonymousObject">The anonymous object</param>
        /// <param name="includeBaseClassProperties">If this is set to true, method will map base class' properties too. </param>
        public static Hash FromAnonymousObject(object anonymousObject, bool includeBaseClassProperties = false)
        {
            Hash result = new Hash();
            if (anonymousObject != null)
            {
                FromAnonymousObject(anonymousObject, result, includeBaseClassProperties);
            }
            return result;
        }

        private static void FromAnonymousObject(object anonymousObject, Hash hash, bool includeBaseClassProperties)
        {
            Action<object, Hash> mapper = GetObjToDictionaryMapper(anonymousObject.GetType(), includeBaseClassProperties);
            mapper.Invoke(anonymousObject, hash);
        }

        private static Action<object, Hash> GetObjToDictionaryMapper(Type type, bool includeBaseClassProperties)
        {
            var cacheKey = type.FullName + "_" + (includeBaseClassProperties ? "WithBaseProperties" : "WithoutBaseProperties");

            return mapperCache.GetOrAdd(cacheKey, (key) => GenerateMapper(type, includeBaseClassProperties));
        }

        private static void AddBaseClassProperties(Type type, List<PropertyInfo> propertyList)
        {
            if (type == null || type == typeof(object))
            {
                return;
            }

            propertyList
                .AddRange(type.GetTypeInfo().DeclaredProperties
                    .Where(
                        p =>
                            p.CanRead &&
                            p.GetMethod.IsPublic &&
                            !p.GetMethod.IsStatic &&
                            propertyList.All(p1 => p1.Name != p.Name))
                    .ToList());

            AddBaseClassProperties(type.GetTypeInfo().BaseType, propertyList);
        }

        private static Action<object, Hash> GenerateMapper(Type type, bool includeBaseClassProperties)
        {
            ParameterExpression objParam = Expression.Parameter(typeof(object), "objParam");
            ParameterExpression hashParam = Expression.Parameter(typeof(Hash), "hashParam");
            List<Expression> bodyInstructions = new List<Expression>();

            var castedObj = Expression.Variable(type, "castedObj");

            bodyInstructions.Add(
                Expression.Assign(castedObj, Expression.Convert(objParam, type))
            );

            //Add properties
            var propertyList = type.GetTypeInfo().DeclaredProperties
                .Where(p => p.CanRead && p.GetMethod.IsPublic && !p.GetMethod.IsStatic).ToList();

            //Add properties from base class
            if (includeBaseClassProperties) AddBaseClassProperties(type, propertyList);

            foreach (PropertyInfo property in propertyList)
            {
                bodyInstructions.Add(
                    Expression.Assign(
                        Expression.MakeIndex(
                            hashParam,
                            typeof(Hash).GetTypeInfo().GetDeclaredProperty("Item"),
                            new[] { Expression.Constant(property.Name, typeof(string)) }
                        ),
                        Expression.Convert(
                            Expression.Property(castedObj, property),
                            typeof(object)
                        )
                    )
                );
            }

            var body = Expression.Block(typeof(void), new[] { castedObj }, bodyInstructions);

            var expr = Expression.Lambda<Action<object, Hash>>(body, objParam, hashParam);

            return expr.Compile();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotLiquid.Hash">Hash</see> class and populates it with the contents of a dictionary
        /// </summary>
        /// <param name="dictionary">The Dictionary object</param>
        public static Hash FromDictionary(IDictionary<string, object> dictionary)
        {
            var hash = new Hash();
            hash.Merge(dictionary);
            return hash;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DotLiquid.Hash">Hash</see> class that is empty and sets the default value
        /// </summary>
        /// <param name="defaultValue">The default value to return if the key lookup fails</param>
        public Hash(object defaultValue)
            : this()
        {
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotLiquid.Hash">Hash</see> class that is empty and sets a method to return a default value
        /// </summary>
        /// <param name="lambda">The method to execute if the key lookup fails</param>
        public Hash(Func<Hash, string, object> lambda)
            : this()
        {
            _lambda = lambda;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotLiquid.Hash">Hash</see> class that is empty
        /// </summary>
        public Hash()
        {
            _nestedDictionary = new Dictionary<string, object>(Template.NamingConvention.StringComparer);
        }

        #endregion

        /// <summary>
        /// Merges a dictionary object into the current <see cref="DotLiquid.Hash">Hash</see>.
        /// </summary>
        /// <param name="otherValues">The dictionary object to be merged into the Hash.</param>
        public void Merge(IDictionary<string, object> otherValues)
        {
            foreach (string key in otherValues.Keys)
                _nestedDictionary[key] = otherValues[key];
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key. If key does not exist, returns the default value, default lambda expression return value, or null.</returns>
        protected virtual object GetValue(string key)
        {
            if (_nestedDictionary.ContainsKey(key))
                return _nestedDictionary[key];

            if (_lambda != null)
                return _lambda(this, key);

            if (_defaultValue != null)
                return _defaultValue;

            return null;
        }

        /// <summary>
        /// Provides strongly-typed access to each of the keys in the Hash
        /// </summary>
        /// <typeparam name="T">A generic parameter that specifies the return type of the column.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value, of type T, associated with the specified key.</returns>
        public T Get<T>(string key)
        {
            return (T)this[key];
        }

        #region IDictionary<string, object>

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _nestedDictionary.GetEnumerator();
        }

        /// <inheritdoc/>
        public void Remove(object key)
        {
            ((IDictionary)_nestedDictionary).Remove(key);
        }

        object IDictionary.this[object key]
        {
            get
            {
                if (!(key is string))
                    throw new NotSupportedException();
                return GetValue((string)key);
            }
            set { ((IDictionary)_nestedDictionary)[key] = value; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _nestedDictionary.GetEnumerator();
        }

        /// <inheritdoc/>
        public void Add(KeyValuePair<string, object> item)
        {
            ((IDictionary<string, object>)_nestedDictionary).Add(item);
        }

        /// <summary>
        /// Determines whether the <see cref="DotLiquid.Hash">Hash</see> contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="DotLiquid.Hash">Hash</see></param>
        /// <returns>true if the <see cref="DotLiquid.Hash">Hash</see> contains an element with the specified key or a default value; otherwise, false.</returns>
        public virtual bool Contains(object key)
        {
            return _lambda != null || _defaultValue != null || ((IDictionary)_nestedDictionary).Contains(key);
        }

        /// <inheritdoc/>
        public void Add(object key, object value)
        {
            ((IDictionary)_nestedDictionary).Add(key, value);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _nestedDictionary.Clear();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)_nestedDictionary).GetEnumerator();
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>)_nestedDictionary).Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((IDictionary<string, object>)_nestedDictionary).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>)_nestedDictionary).Remove(item);
        }

        #endregion

        #region IDictionary

        /// <inheritdoc/>
        public void CopyTo(Array array, int index)
        {
            ((IDictionary)_nestedDictionary).CopyTo(array, index);
        }

        /// <inheritdoc/>
        public int Count
        {
            get { return _nestedDictionary.Count; }
        }

        /// <inheritdoc/>
        public object SyncRoot
        {
            get { return ((IDictionary)_nestedDictionary).SyncRoot; }
        }

        /// <inheritdoc/>
        public bool IsSynchronized
        {
            get { return ((IDictionary)_nestedDictionary).IsSynchronized; }
        }

        ICollection IDictionary.Values
        {
            get { return ((IDictionary)_nestedDictionary).Values; }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get { return ((IDictionary<string, object>)_nestedDictionary).IsReadOnly; }
        }

        /// <inheritdoc />
        public bool IsFixedSize
        {
            get { return ((IDictionary)_nestedDictionary).IsFixedSize; }
        }

        /// <summary>
        /// Determines whether the <see cref="DotLiquid.Hash">Hash</see> contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="DotLiquid.Hash">Hash</see></param>
        /// <returns>true if the <see cref="DotLiquid.Hash">Hash</see> contains an element with the specified key or a default value; otherwise, false.</returns>
        public bool ContainsKey(string key)
        {
            return _lambda != null || _defaultValue != null || _nestedDictionary.ContainsKey(key);
        }

        /// <inheritdoc/>
        public void Add(string key, object value)
        {
            _nestedDictionary.Add(key, value);
        }

        /// <inheritdoc/>
        public bool Remove(string key)
        {
            return _nestedDictionary.Remove(key);
        }

        /// <inheritdoc/>
        public bool TryGetValue(string key, out object value)
        {
            return _nestedDictionary.TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        public object this[string key]
        {
            get { return GetValue(key); }
            set { _nestedDictionary[key] = value; }
        }

        /// <inheritdoc/>
        public ICollection<string> Keys
        {
            get { return _nestedDictionary.Keys; }
        }

        ICollection IDictionary.Keys
        {
            get { return ((IDictionary)_nestedDictionary).Keys; }
        }

        /// <inheritdoc/>
        public ICollection<object> Values
        {
            get { return _nestedDictionary.Values; }
        }

        #endregion
    }
}
