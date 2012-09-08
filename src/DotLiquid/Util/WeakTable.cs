using System;

namespace DotLiquid.Util
{
	internal class WeakTable<TKey, TValue> where TValue : class
	{
		private struct Bucket
		{
			public TKey Key;
			public WeakReference Value;
		}

		private readonly Bucket[] _buckets;

		public WeakTable(int size)
		{
			_buckets = new Bucket[size];
		}

		public TValue this[TKey key]
		{
			get
			{
				TValue ret;
				if (!TryGetValue(key, out ret))
					throw new ArgumentException(Liquid.ResourceManager.GetString("WeakTableKeyNotFoundException"));
				return ret;
			}
			set
			{
				int i = Math.Abs(key.GetHashCode()) % _buckets.Length;
				_buckets[i].Key = key;
				_buckets[i].Value = new WeakReference(value);
			}
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			int i = Math.Abs(key.GetHashCode()) % _buckets.Length;
			WeakReference wr;
			if ((wr = _buckets[i].Value) == null || !_buckets[i].Key.Equals(key))
			{
				value = null;
				return false;
			}
			value = (TValue)wr.Target;
			return wr.IsAlive;
		}

		public void Remove(TKey key)
		{
			int i = Math.Abs(key.GetHashCode()) % _buckets.Length;
			if (_buckets[i].Key.Equals(key))
				_buckets[i].Value = null;
		}
	}
}