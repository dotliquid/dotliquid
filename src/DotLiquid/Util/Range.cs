using System;
using System.Collections.Generic;

namespace DotLiquid.Util
{
	/// <summary>
	/// Taken from code at http://www.pluralsight-training.net/community/blogs/dbox/archive/2005/04/24/7690.aspx.
	/// </summary>
	internal static class Range
	{
		#region Successor functions

		internal static long Succ(long val)
		{
			return val + 1;
		}

		internal static int Succ(int val)
		{
			return val + 1;
		}

		internal static short Succ(short val)
		{
			return (short) (val + 1);
		}

		internal static sbyte Succ(sbyte val)
		{
			return (sbyte) (val + 1);
		}

		internal static ulong Succ(ulong val)
		{
			return val + 1;
		}

		internal static uint Succ(uint val)
		{
			return val + 1;
		}

		internal static ushort Succ(ushort val)
		{
			return (ushort) (val + 1);
		}

		internal static byte Succ(byte val)
		{
			return (byte) (val + 1);
		}

		internal static char Succ(char val)
		{
			return (char) (val + 1);
		}

		internal static DateTime Succ(DateTime val)
		{
			return val.AddDays(1);
		}

		internal static string Succ(string val)
		{
			int lastAlphaNumeric = -1;
			for (int i = val.Length - 1; i >= 0 && lastAlphaNumeric == -1; i--)
			{
				if (char.IsLetterOrDigit(val[i]))
					lastAlphaNumeric = i;
			}
			if (lastAlphaNumeric == val.Length - 1 || lastAlphaNumeric == -1)
				return Succ(val, val.Length);
			return Succ(val, lastAlphaNumeric + 1) + val.Substring(lastAlphaNumeric + 1);
		}

		internal static string Succ(string val, int length)
		{
			char lastChar = val[length - 1];
			switch (lastChar)
			{
				case '9':
					return ((length > 1) ? Succ(val, length - 1) : "1") + '0';
				case 'z':
					return ((length > 1) ? Succ(val, length - 1) : "a") + 'a';
				case 'Z':
					return ((length > 1) ? Succ(val, length - 1) : "A") + 'A';
				default:
					return val.Substring(0, length - 1) + (char) (lastChar + 1);
			}
		}

		#endregion

		public static IEnumerable<T> Inclusive<T>(T start, T finish, Converter<T, T> succ, Comparison<T> comp)
		{
			T value = start;
			while (comp(value, finish) <= 0)
			{
				yield return value;
				value = succ(value);
			}
		}

		internal static int Comp<T>(T a, T b) where T : IComparable<T>
		{
			if (a != null)
				return a.CompareTo(b);
			return (b == null) ? 0 : -1;
		}

		public static IEnumerable<T> Inclusive<T>(T start, T finish, Converter<T, T> succ)
			where T : IComparable<T>
		{
			return Inclusive<T>(start, finish, succ, Comp);
		}

		public static IEnumerable<DateTime> Inclusive(DateTime start, DateTime finish)
		{
			return Inclusive(start, finish, Succ, Comp);
		}

		public static IEnumerable<string> Inclusive(string start, string finish)
		{
			return Inclusive(start, finish, Succ, Comp);
		}

		public static IEnumerable<int> Inclusive(int start, int finish)
		{
			return Inclusive(start, finish, Succ, Comp);
		}
	}
}