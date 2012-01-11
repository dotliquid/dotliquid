using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace DotLiquid.Tests.Framework
{
	public static class ExtendedCollectionAssert
	{
		/// <summary>
		/// Asserts that all items contained in collection are of the types specified by expectedTypes. 
		/// </summary>
		/// <param name="collection">IEnumerable containing objects to be considered</param>
		/// <param name="expectedTypes"></param>
		public static void AllItemsAreInstancesOfTypes(IEnumerable collection, IEnumerable<Type> expectedTypes)
		{
			IEnumerator collectionEnumerator = collection.GetEnumerator();
			IEnumerator<Type> typesEnumerator = expectedTypes.GetEnumerator();

			while (collectionEnumerator.MoveNext() && typesEnumerator.MoveNext())
				Assert.IsInstanceOf(typesEnumerator.Current, collectionEnumerator.Current);
		}
	}
}