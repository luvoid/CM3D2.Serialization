using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal static class AssertExtensions
{
	public static void ArraysAreEqual<T>(this Assert _, in T[] expected, in T[] actual)
	{
		Assert.AreEqual(expected.Length, actual.Length);
		for (int i = 0; i < expected.Length; i++)
		{
			Assert.AreEqual(expected[i], actual[i]);
		}
	}
}
