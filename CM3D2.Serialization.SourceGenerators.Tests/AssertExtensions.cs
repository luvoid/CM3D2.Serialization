﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

internal static class AssertExtensions
{
	public static void ArraysAreEqual<T>(this Assert _, in T[] expected, in T[] actual, string message = "")
	{
		Assert.AreEqual(expected.Length, actual.Length, $"Lengths don't match. {message}");
		for (int i = 0; i < expected.Length; i++)
		{
			Assert.AreEqual(expected[i], actual[i], $"Index:<{i}>. {message}");
		}
	}

	public static void StreamsAreEqual(this Assert _, in Stream expected, in Stream actual, string message = "")
	{
		while ((expected.Position < expected.Length) && (actual.Position < actual.Length))
		{
			long offset = expected.Position;
			int sizeExpected = (int)Math.Min(expected.Length - expected.Position, 64);
			int sizeActual   = (int)Math.Min(actual  .Length - actual  .Position, 64);
			byte[] arrayExpected = new byte[sizeExpected];
			byte[] arrayActual   = new byte[sizeActual  ];

			expected.Read(arrayExpected, 0, arrayExpected.Length);
			actual  .Read(arrayActual  , 0, arrayActual  .Length);

			Assert.That.ArraysAreEqual(arrayExpected, arrayActual, $"Offset:<0x{offset:X6}>. {message}\n" +
				$"Expected:<{arrayExpected.ToHexString()}>.\n" +
				$"Actual  :<{arrayActual  .ToHexString()}>.");
		}
	}

	public static string ToHexString(this byte[] bytes)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (byte b in bytes)
		{
			stringBuilder.Append($"{b:X2} ");
		}
		return stringBuilder.ToString(0, stringBuilder.Length - 1);
	}

	public static void ThrewNoExceptions<TKey, TValue>(this Assert _, in IEnumerable<KeyValuePair<TKey, TValue>> sourceExceptions)
		where TValue : IEnumerable<Exception>
	{
		string error = string.Empty;
		foreach (var pair in sourceExceptions)
		{
			var source = pair.Key;
			var exceptions = pair.Value;
			foreach (var exception in exceptions)
			{
				error += $"{source} threw {exception}\n";
			}
		}

		if (error != string.Empty)
		{
			throw new AssertFailedException(error);
		}
	}

	public static void ThrewNoExceptions(this Assert _, in GeneratorDriverRunResult runResult)
	{
		string error = string.Empty;
		foreach (var result in runResult.Results)
		{
			if (result.Exception == null) continue;
			error += $"Generator {result.Generator.GetGeneratorType().Name} threw {result.Exception}";
		}

		if (error != string.Empty)
		{
			throw new AssertFailedException(error);
		}
	}
}
