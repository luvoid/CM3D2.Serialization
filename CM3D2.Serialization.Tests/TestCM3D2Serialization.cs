using CM3D2.Serialization.Files;
using CM3D2.Serialization.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace CM3D2.Serialization.Tests
{
	[TestClass]
	public class TestCM3D2Serialization
	{
		MemoryStream m_Stream = new MemoryStream();
		CM3D2Formatter m_Formatter = new CM3D2Formatter();


		[TestMethod]
		public void TestSerializeBool()
		{
			bool expected = true;

			m_Formatter.Serialize(m_Stream, expected);


			m_Stream.Position = 0; // Reset stream position


			byte[] bytes = ReadBytes();
			PrintBytes(bytes);
			bool result = BitConverter.ToBoolean(bytes, 0);
			Console.WriteLine(result);

			Assert.IsTrue(bytes.Length == 1, $"Bool was written as {bytes.Length} bytes, but must only be written as 1 byte.");
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TestSerializeInt()
		{
			int expected = 1234567890;

			m_Formatter.Serialize(m_Stream, expected);


			m_Stream.Position = 0; // Reset stream position


			byte[] bytes = ReadBytes();
			PrintBytes(bytes);
			int result = BitConverter.ToInt32(bytes, 0);
			Console.WriteLine(result);


			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TestDeserializeInt()
		{
			int expected = 1234567890;

			byte[] bytes = BitConverter.GetBytes(expected);
			m_Stream.Write(bytes, 0, bytes.Length);


			m_Stream.Position = 0; // Reset stream position


			int result = m_Formatter.Deserialize<int>(m_Stream);
			Console.WriteLine(result);


			Assert.AreEqual(expected, result);
		}


		[TestMethod]
		public void TestSerializeStruct()
		{
			Float3 expected = new Float3(0.1f, 2.3f, 4.5f);
			Console.WriteLine($"expected = {expected}");

			m_Formatter.Serialize(m_Stream, expected);


			m_Stream.Position = 0; // Reset stream position


			byte[] bytes = ReadBytes();
			PrintBytes(bytes);
			float resultX = BitConverter.ToSingle(bytes, 0);
			float resultY = BitConverter.ToSingle(bytes, 4);
			float resultZ = BitConverter.ToSingle(bytes, 8);
			Console.WriteLine($"result = ({resultX}, {resultY}, {resultZ})");


			Assert.AreEqual(expected.x, resultX);
			Assert.AreEqual(expected.y, resultY);
			Assert.AreEqual(expected.z, resultZ);
		}


		[TestMethod]
		public void TestDeserializeStruct()
		{
			float expectedX = 0.1f;
			float expectedY = 2.3f;
			float expectedZ = 4.5f;
			Console.WriteLine($"expected = ({expectedX}, {expectedY}, {expectedZ})");

			byte[] bytes0 = BitConverter.GetBytes(expectedX);
			byte[] bytes4 = BitConverter.GetBytes(expectedY);
			byte[] bytes8 = BitConverter.GetBytes(expectedZ);
			m_Stream.Write(bytes0, 0, bytes0.Length);
			m_Stream.Write(bytes4, 0, bytes4.Length);
			m_Stream.Write(bytes8, 0, bytes8.Length);


			m_Stream.Position = 0; // Reset stream position


			Float3 result = m_Formatter.Deserialize<Float3>(m_Stream);
			Console.WriteLine($"result = {result}");


			Assert.AreEqual(expectedX, result.x);
			Assert.AreEqual(expectedY, result.y);
			Assert.AreEqual(expectedZ, result.z);
		}


		[TestMethod]
		public void TestSerializeString()
		{
			string testString = "CM3D2_MODEL";
			byte[] expected = { 11, (byte)'C', (byte)'M', (byte)'3', (byte)'D', (byte)'2', (byte)'_', (byte)'M', (byte)'O', (byte)'D', (byte)'E', (byte)'L' };

			m_Formatter.Serialize(m_Stream, testString);


			m_Stream.Position = 0; // Reset stream position


			byte[] result = ReadBytes();
			PrintBytes(result);
			for (int i = 1; i-1 < result[0]; i++)
			{
				Console.Write((char)result[i]);
			}
			Console.WriteLine();


			Assert.That.ArraysAreEqual(expected, result);
		}

		[TestMethod]
		public void TestDeserializeString()
		{
			string expected = "CM3D2_MODEL";
			byte[] testArray = { 11, (byte)'C', (byte)'M', (byte)'3', (byte)'D', (byte)'2', (byte)'_', (byte)'M', (byte)'O', (byte)'D', (byte)'E', (byte)'L' };
			m_Stream.Write(testArray, 0, testArray.Length);


			m_Stream.Position = 0; // Reset stream position


			string result = m_Formatter.Deserialize<string>(m_Stream);


			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TestSerializeAnm()
		{
			Anm anm = new Anm();

			m_Formatter.Serialize(m_Stream, anm);


			m_Stream.Position = 0; // Reset stream position


			byte[] result = ReadBytes();
			PrintBytes(result);
		}


		byte[] ReadBytes(int count = 4096, Stream stream = null)
		{
			if (stream == null) stream = m_Stream;
			var bytes = new byte[count];
			int readCount = stream.Read(bytes, 0, count);
			Array.Resize(ref bytes, readCount);
			return bytes;
		}

		void PrintBytes(byte[] bytes)
		{
			foreach (byte b in bytes)
			{
				Console.Write($"{b:X2} ");
			}
			Console.WriteLine();
		}


	}
}
