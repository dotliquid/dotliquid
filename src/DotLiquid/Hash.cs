using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DotLiquid
{
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
        /// 
        /// </summary>
        /// <param name="anonymousObject"></param>
        /// <param name="includeBaseClassProperties">If this is set to true, method will map base class' properties too. </param>
        /// <returns></returns>
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

            if (!mapperCache.TryGetValue(cacheKey, out Action<object, Hash> mapper))
            {
                /* Bogdan Mart: Note regarding concurrency:
                 * This is concurrent dictionary, but if this will be called from two threads
                 * this code would generate two same mappers, which will cause some CPU overhead.
                 * But I have no idea on what I can lock here, first thought was to use lock(type),
                 * but that could cause deadlock, if some outside code will lock Type.
                 * Only correct solution would be to use ConcurrentDictionary<Type, Action<object, Hash>>
                 * with some CAS race, and then locking, or Semaphore, but first will add complexity, 
                 * second would add overhead in locking on Kernel-level named object.
                 * 
                 * So I assume tradeoff in not using locks here is better, 
                 * we at most will waste some CPU cycles on code generation, 
                 * but RAM would be collected, due to http://stackoverflow.com/questions/5340201/
                 * 
                 * If someone have conserns, than one can lock(mapperCache) but that would 
                 * create bottleneck, as only one mapper could be generated at a time.
                 */
                mapper = GenerateMapper(type, includeBaseClassProperties);
                mapperCache[cacheKey] = mapper;
            }

            return mapper;
        }

        private static void AddBaseClassProperties(Type type, List<PropertyInfo> propertyList) {
            propertyList.AddRange(type.GetTypeInfo().BaseType.GetTypeInfo().DeclaredProperties
                .Where(p => p.CanRead && p.GetMethod.IsPublic && !p.GetMethod.IsStatic).ToList());
        }

        private static Action<object, Hash> GenerateMapper(Type type, bool includeBaseClassProperties)
        {
            ParameterExpression objParam = Expression.Parameter(typeof(object), "objParam");
            ParameterExpression hashParam = Expression.Parameter(typeof(Hash), "hashParam");
            List<Expression> bodyInstructions = new List<Expression>();

            var castedObj = Expression.Variable(type,"castedObj");
            
            bodyInstructions.Add(
                Expression.Assign(castedObj,Expression.Convert(objParam,type))
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
                            new []{Expression.Constant(property.Name, typeof(string))}
                        ),
                        Expression.Convert(
                            Expression.Property(castedObj,property),
                            typeof(object)
                        )
                    )
                );
            }

            var body = Expression.Block(typeof(void),new []{castedObj},bodyInstructions);

            var expr = Expression.Lambda < Action<object, Hash>>(body, objParam, hashParam);

            return expr.Compile();
        }

        public static Hash FromDictionary(IDictionary<string, object> dictionary)
        {
            Hash result = new Hash();

            foreach (var keyValue in dictionary)
            {
                    if (keyValue.Value is IDictionary<string, object>)
                    {
                        result.Add(keyValue.Key, FromDictionary((IDictionary<string, object>) keyValue.Value));
                    }
                    else
                    {
                        result.Add(keyValue);
                    }
            }
                
            return result;
        }

        #endregion

        #region Constructors

        public Hash(object defaultValue)
            : this()
        {
            _defaultValue = defaultValue;
        }

        public Hash(Func<Hash, string, object> lambda)
            : this()
        {
            _lambda = lambda;
        }

        public Hash()
        {
            _nestedDictionary = new Dictionary<string, object>(Template.NamingConvention.StringComparer);
        }

        #endregion

        public void Merge(IDictionary<string, object> otherValues)
        {
            foreach (string key in otherValues.Keys)
                _nestedDictionary[key] = otherValues[key];
        }

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

        public T Get<T>(string key)
        {
            return (T) this[key];
        }

        #region IDictionary<string, object>

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _nestedDictionary.GetEnumerator();
        }

        public void Remove(object key)
        {
            ((IDictionary) _nestedDictionary).Remove(key);
        }

        object IDictionary.this[object key]
        {
            get
            {
                if (!(key is string))
                    throw new NotSupportedException();
                return GetValue((string) key);
            }
            set { ((IDictionary) _nestedDictionary)[key] = value; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _nestedDictionary.GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            ((IDictionary<string, object>) _nestedDictionary).Add(item);
        }

        public virtual bool Contains(object key)
        {
            return ((IDictionary) _nestedDictionary).Contains(key);
        }

        public void Add(object key, object value)
        {
            ((IDictionary) _nestedDictionary).Add(key, value);
        }

        public void Clear()
        {
            _nestedDictionary.Clear();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary) _nestedDictionary).GetEnumerator();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>) _nestedDictionary).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((IDictionary<string, object>) _nestedDictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>) _nestedDictionary).Remove(item);
        }

        #endregion

        #region IDictionary

        public void CopyTo(Array array, int index)
        {
            ((IDictionary) _nestedDictionary).CopyTo(array, index);
        }

        public int Count
        {
            get { return _nestedDictionary.Count; }
        }

        public object SyncRoot
        {
            get { return ((IDictionary) _nestedDictionary).SyncRoot; }
        }

        public bool IsSynchronized
        {
            get { return ((IDictionary) _nestedDictionary).IsSynchronized; }
        }

        ICollection IDictionary.Values
        {
            get { return ((IDictionary) _nestedDictionary).Values; }
        }

        public bool IsReadOnly
        {
            get { return ((IDictionary<string, object>) _nestedDictionary).IsReadOnly; }
        }

        public bool IsFixedSize
        {
            get { return ((IDictionary) _nestedDictionary).IsFixedSize; }
        }

        public bool ContainsKey(string key)
        {
            return _nestedDictionary.ContainsKey(key);
        }

        public void Add(string key, object value)
        {
            _nestedDictionary.Add(key, value);
        }

        public bool Remove(string key)
        {
            return _nestedDictionary.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _nestedDictionary.TryGetValue(key, out value);
        }

        public object this[string key]
        {
            get { return GetValue(key); }
            set { _nestedDictionary[key] = value; }
        }

        public ICollection<string> Keys
        {
            get { return _nestedDictionary.Keys; }
        }

        ICollection IDictionary.Keys
        {
            get { return ((IDictionary) _nestedDictionary).Keys; }
        }

        public ICollection<object> Values
        {
            get { return _nestedDictionary.Values; }
        }

        #endregion
    }
}
