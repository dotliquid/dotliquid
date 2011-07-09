using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace DotLiquid
{
	public class Hash : IDictionary<string, object>, IDictionary
	{
		#region Fields

		private readonly Func<Hash, string, object> _lambda;
		private readonly Dictionary<string, object> _nestedDictionary;
		private readonly object _defaultValue;

		#endregion

		#region Static construction methods

		public static Hash FromAnonymousObject(object anonymousObject)
		{
			Hash result = new Hash();
			if (anonymousObject != null)
				foreach (PropertyInfo property in anonymousObject.GetType().GetProperties())
					result[property.Name] = property.GetValue(anonymousObject, null);
			return result;
		}

		public static Hash FromDictionary(IDictionary<string, object> dictionary)
		{
			Hash result = new Hash();
			foreach (var keyValue in dictionary)
				result.Add(keyValue);
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

		private object GetValue(string key)
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

		public bool Contains(object key)
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