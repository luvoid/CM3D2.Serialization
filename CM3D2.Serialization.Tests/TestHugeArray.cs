using CM3D2.Serialization.Collections;
using CM3D2.Serialization.Collections.Generics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;


namespace CM3D2.Serialization.Tests
{
	[TestClass]
	public class TestHugeArray
	{
		void TestIndex(long index, bool expectInvalid = false, bool print = true)
		{
			int xIndexExpected = (int)(index / (int.MaxValue + 1L));
			int yIndexExpected = (int)(index % (int.MaxValue + 1L));

			bool isValidIndex = HugeArray<object>.TranslateIndex(index, out int xIndexActual, out int yIndexActual);

			string msg = $"For {index}L (0b{Bits(index)}) " +
				$"Expected: 0b{Bits(xIndexExpected):B32}_{Bits(yIndexExpected):B32}  " +
				$"Actual:   0b{Bits(xIndexActual  ):B32}_{Bits(yIndexActual  ):B32}";

			try
			{
				Assert.AreEqual(xIndexExpected, xIndexActual, msg);
				Assert.AreEqual(yIndexExpected, yIndexActual, msg);
			}
			catch (Exception ex)
			{
				// Always follow the assertion if this is a valid index
				if (isValidIndex) throw ex;

				// Always follow the assertion if an invalid index was not expected
				if (!expectInvalid) throw ex;
			}
			Assert.AreEqual(!expectInvalid, isValidIndex, $"Unexpected index validity. {msg}");

			if (print)
			{
				Console.WriteLine(msg);
			}
		}

		[TestMethod]
		public void TestTranslateIndex()
		{
			// Common edge cases

			TestIndex(0);
			TestIndex(1);
			TestIndex(2);

			TestIndex(int.MaxValue - 1L);
			TestIndex(int.MaxValue + 0L);
			TestIndex(int.MaxValue + 1L);

			TestIndex((long.MaxValue >> 1) - 1L);
			TestIndex((long.MaxValue >> 1) + 0L);


			// These indices should be invalid

			TestIndex((long.MaxValue >> 1) + 1L, expectInvalid: true);

			TestIndex(long.MaxValue - 2L, expectInvalid: true);
			TestIndex(long.MaxValue - 1L, expectInvalid: true);
			TestIndex(long.MaxValue - 0L, expectInvalid: true);

			TestIndex(-1, expectInvalid: true);
		}

		//[TestMethod]
		public void TestTranslateIndexFull()
		{
			// This will take a really long time...
			for (long l = 0; l < HugeArray<object>.MAX_INDEX; l++)
			{
				TestIndex(l, print: false);
			}
		}

		[TestMethod]
		public void TestBits()
		{
			for (int i=-8; i<8; i++)
			{
				Console.WriteLine($"{i} : 0b{Bits(i, digits: 4)}");
			}
		}

		static string Bits(int num, int digits = 32)
		{
			bool neg = num < 0;
			if (neg) num = -num-1;
			char[] chars = new char[digits];
			for (int i = digits - 1; i >= 0; i--)
			{
				int rem = num % 2;
				num = num / 2;
				chars[i] = (rem == 0) ^ neg ? '0' : '1';
			}
			return new string(chars);
		}

		static string Bits(long num, int digits = 64)
		{
			bool neg = num < 0;
			if (neg) num = -num - 1;
			char[] chars = new char[digits];
			for (int i = digits - 1; i >= 0; i--)
			{
				long rem = num % 2;
				num = num / 2;
				chars[i] = (rem == 0) ^ neg ? '0' : '1';
			}
			return new string(chars).Insert(32, "_");
		}
	}
}
