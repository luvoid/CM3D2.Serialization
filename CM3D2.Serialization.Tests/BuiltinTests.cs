using CM3D2.Serialization.Files;
using CM3D2.Serialization.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace CM3D2.Serialization.Tests
{
    [TestClass]
	public class BuiltinTests : CommonTests
	{
		[TestMethod]
		public void TestSerializeBool()
		{
			bool expected = true;

			m_Serializer.Serialize(m_Stream, expected);


			m_Stream.Position = 0; // Reset stream position


			byte[] bytes = ReadBytes();
			Console.WriteLine(bytes.ToHexString());
			bool result = BitConverter.ToBoolean(bytes, 0);
			Console.WriteLine(result);

			Assert.IsTrue(bytes.Length == 1, $"Bool was written as {bytes.Length} bytes, but must only be written as 1 byte.");
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TestSerializeInt()
		{
			int expected = 1234567890;

			m_Serializer.Serialize(m_Stream, expected);


			m_Stream.Position = 0; // Reset stream position


			byte[] bytes = ReadBytes();
			Console.WriteLine(bytes.ToHexString());
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


			int result = m_Serializer.Deserialize<int>(m_Stream);
			Console.WriteLine(result);


			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TestSerializeUShort()
		{
			ushort expected = 12345;

			m_Serializer.Serialize(m_Stream, expected);


			m_Stream.Position = 0; // Reset stream position


			byte[] bytes = ReadBytes();
			Console.WriteLine(bytes.ToHexString());
			int result = BitConverter.ToUInt16(bytes, 0);
			Console.WriteLine(result);


			Assert.AreEqual(expected, result);
			Assert.AreEqual(bytes.Length, 2, "Not serialized as correct number of bytes");
		}

		[TestMethod]
		public void TestDeserializeUShort()
		{
			ushort expected = 12345;

			byte[] bytes = BitConverter.GetBytes(expected);
			m_Stream.Write(bytes, 0, bytes.Length);

			Console.WriteLine($"sizeof(ushort) = {sizeof(ushort)}");


			m_Stream.Position = 0; // Reset stream position


			int result = m_Serializer.Deserialize<ushort>(m_Stream);
			Console.WriteLine(result);


			Assert.AreEqual(expected, result);
			Assert.AreEqual(bytes.Length, 2, "Not deserialized as correct number of bytes");
		}


		[TestMethod]
		public void TestSerializeStruct()
		{
			Float3 expected = new Float3(0.1f, 2.3f, 4.5f);
			Console.WriteLine($"expected = {expected}");

			m_Serializer.Serialize(m_Stream, expected);


			m_Stream.Position = 0; // Reset stream position


			byte[] bytes = ReadBytes();
			Console.WriteLine(bytes.ToHexString());
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


			Float3 result = m_Serializer.Deserialize<Float3>(m_Stream);
			Console.WriteLine($"result = {result}");


			Assert.AreEqual(expectedX, result.x);
			Assert.AreEqual(expectedY, result.y);
			Assert.AreEqual(expectedZ, result.z);
		}


		[TestMethod]
		public void TestSerializeEnum()
		{
			ShortEnum expected = ShortEnum.Value256;

			m_Serializer.Serialize(m_Stream, expected);


			m_Stream.Position = 0; // Reset stream position


			byte[] bytes = ReadBytes();
			Console.WriteLine(bytes.ToHexString());
			short result = BitConverter.ToInt16(bytes, 0);
			Console.WriteLine(result);
			

			Assert.AreEqual(expected, (ShortEnum)result);
		}



		[TestMethod]
		public void TestDeserializeEnum()
		{
			ShortEnum expected = ShortEnum.Value256;
			short value = (short)expected;
			m_Stream.WriteByte((byte)(value & 255));
			m_Stream.WriteByte((byte)(value >> 8));


			m_Stream.Position = 0; // Reset stream position


			ShortEnum result = m_Serializer.Deserialize<ShortEnum>(m_Stream);


			Assert.AreEqual(expected, result);
		}


		[TestMethod]
		public void TestSerializeString()
		{
			string testString = "CM3D2_MODEL";
			byte[] expected = { 11, (byte)'C', (byte)'M', (byte)'3', (byte)'D', (byte)'2', (byte)'_', (byte)'M', (byte)'O', (byte)'D', (byte)'E', (byte)'L' };

			m_Serializer.Serialize(m_Stream, testString);


			m_Stream.Position = 0; // Reset stream position


			byte[] result = ReadBytes();
			Console.WriteLine(result.ToHexString());
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


			string result = m_Serializer.Deserialize<string>(m_Stream);


			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TestLongString()
		{
			string expected = "0123456789" +
				"ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
				"abcdefghijklmnopqrstuvwxyz" +
				"０１２３４５６７８９" +
				"ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ" +
				"ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ" +
				"ぁあぃいぅうぇえぉおかがきぎく" +
				"ぐけげこごさざしじすずせぜそぞた" +
				"だちぢっつづてでとどなにぬねのは" +
				"ばぱひびぴふぶぷへべぺほぼぽまみ" +
				"むめもゃやゅゆょよらりるれろゎわ " +
				"ゐゑをんゔゕゖ゛゜ゝゞゟ";


			m_Serializer.Serialize(m_Stream, expected);

			m_Stream.Position = 0; // Reset stream position

			string result = m_Serializer.Deserialize<string>(m_Stream);

			Assert.AreEqual(expected, result);
		}


	}
}
