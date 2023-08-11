using CM3D2.Serialization.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CM3D2.Serialization.Tests
{
	public abstract class FileTests<T> : CommonTests
		where T : class, ICM3D2Serializable, ISummarizable
	{
		protected abstract T CreateExample();

		protected abstract string ExampleFilePath { get; }

		[TestMethod]
		public abstract void SerializeTest();
		public void DoSerializeTest()
		{
			T example = CreateExample();

			m_Serializer.Serialize(m_Stream, example);


			m_Stream.Position = 0; // Reset stream position


			byte[] result = ReadBytes();
			Console.WriteLine(result.ToHexString());
		}

		[TestMethod]
		public abstract void DeserializeTest();
		public void DoDeserializeTest()
		{
			string filePath = ExampleFilePath;
			T example;
			using (var file = new DebugFileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				example = m_Serializer.Deserialize<T>(file);
			}

			Console.WriteLine($"{filePath}: {example.Summarize()}");
		}

		[TestMethod]
		public abstract void SerializeAndDeserializeTest();
		public void DoSerializeAndDeserializeTest()
		{
			T example = CreateExample();

			// Serialize
			m_Serializer.Serialize(m_Stream, example);

			string summaryA = example.Summarize();
			Console.WriteLine($"{typeof(T).Name.ToLower()} = {summaryA}");


			m_Stream.Position = 0; // Reset stream position


			// Deserialize
			example = m_Serializer.Deserialize<T>(m_Stream);

			string summaryB = example.Summarize();
			Console.WriteLine($"{typeof(T).Name.ToLower()} = {summaryB}");

			Assert.AreEqual(summaryA, summaryB);
		}
		
		[TestMethod]
		public abstract void UnchangedTest();
		public void DoUnchangedTest()
		{
			string filePath = ExampleFilePath;
			T example;

			// Deserialize File
			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				example = m_Serializer.Deserialize<T>(file);
			}

			//Console.WriteLine($"{typeof(T).Name.ToLower()}.signature = \"{example.signature}\"");

			// Serialize Example
			var tempFilePath = Path.GetTempFileName(); // Use a temporary file
			Console.WriteLine($"Using temporary file {tempFilePath}");
			try
			{
				using (var tempFile = new FileStream(tempFilePath, FileMode.Open, FileAccess.Write))
				{
					m_Serializer.Serialize(tempFile, example);
				}

				// Compare the two streams
				using (var expectedFile = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				{
					using (var actualFile = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
					{
						Assert.That.StreamsAreEqual(expectedFile, actualFile);
					}
				}

			}
			finally
			{
				File.Delete(tempFilePath);
			}

		}
	}
}
