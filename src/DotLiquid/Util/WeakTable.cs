using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotLiquid.Util
{
    class WeakTable<TKey, TValue> where TValue : class
    {
        private struct Bucket
        {
            public TKey Key;
            public WeakReference Value;
        }

        Bucket[] _buckets;

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
                    throw new ArgumentException("key could not be found");
                return ret;
            }
            set
            {
                int i = key.GetHashCode() % _buckets.Length;
                _buckets[i].Key = key;
                _buckets[i].Value = new WeakReference(value);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int i = key.GetHashCode() % _buckets.Length;
            WeakReference wr;
            if (!_buckets[i].Key.Equals(key) || (wr = _buckets[i].Value) == null)
            {
                value = null;
                return false;
            }
            value = (TValue)wr.Target;
            return wr.IsAlive;
        }

        public void Remove(TKey key)
        {
            int i = key.GetHashCode() % _buckets.Length;
            if (_buckets[i].Key.Equals(key))
                _buckets[i].Value = null;
        }
    }
}
