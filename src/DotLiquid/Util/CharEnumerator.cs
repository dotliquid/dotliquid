using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DotLiquid.Util
{
    /// <summary>
    /// CharEnumerator is a similar implementation to the native class by the same name but is
    /// (a) compatible with .net standard 1.3
    /// (b) exposes additional properties like Position (Stream-like) Next (to peek) etc.
    /// </summary>
    internal class CharEnumerator : IEnumerator<char>
    {
        private readonly String str;
        private int index;

        internal CharEnumerator(String str)
        {
            if (str == null)
                throw new ArgumentException("String must not be null", nameof(str));
            this.str = str;
            this.index = -1;
        }

        public bool MoveNext()
        {
            if (index < (str.Length - 1))
            {
                index++;
                return true;
            }
            else
                index = str.Length;
            return false;
        }

        public bool AppendNext(StringBuilder sb)
        {
            if (!MoveNext())
                return false;

            sb.Append(Current);
            return true;
        }

        public bool HasNext() => index < (str.Length - 1);

        /// <internalonly/>
        Object IEnumerator.Current => str[index];

        public char Current => str[index];

        public char Previous => str[index - 1];

        public char Next => str[index + 1];

        public int Remaining => str.Length == index ? 0 : str.Length - index - 1;

        public int Position => index + 1;

        public void Reset()
        {
            index = -1;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                index = str.Length;
            }
        }
    }
}
