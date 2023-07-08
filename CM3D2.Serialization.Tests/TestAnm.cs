using CM3D2.Serialization.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace CM3D2.Serialization.Tests
{

	[TestClass]
	public class TestAnm : CommonTest
	{
		Anm CreateTestAnm()
		{
			Anm anm = new Anm();
			anm.tracks.Add(new Anm.Track());
			anm.tracks[0].path = "path/to/bone";
			anm.tracks[0].channels.Add(new Anm.Track.Channel());
			anm.tracks[0].channels[0].keyframes = new Collections.LengthPrefixedArray<Anm.Track.Channel.Keyframe>(4);

			return anm;
		}

		const string k_ExamplePosePath = "Resources/T-Pose.anm";
		const string k_ExampleActionPath = "Resources/push_ups.anm";
		//const string k_ExampleActionPath = "Resources/maid_stand01.anm";
		//const string k_ExampleActionPath = "Resources/dance_cm3d21_008_1oy_c01_02_f1.anm";

		[TestMethod]
		public void TestSerializeAnm()
		{
			Anm anm = CreateTestAnm();

			m_Serializer.Serialize(m_Stream, anm);


			m_Stream.Position = 0; // Reset stream position


			byte[] result = ReadBytes();
			Console.WriteLine(result.ToHexString());
		}

		[TestMethod]
		public void TestDeserializeAnm()
		{
			string filePath = k_ExamplePosePath;
			Anm anm;
			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				anm = m_Serializer.Deserialize<Anm>(file);
			}

			Console.WriteLine($"{filePath}: {anm.Summarize()}");
		}

		[TestMethod]
		public void TestDeserializePoseKeyframes()
		{
			string filePath = k_ExamplePosePath;
			Anm anm;
			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				anm = m_Serializer.Deserialize<Anm>(file);
			}


			Console.WriteLine($"{filePath}: {anm.Summarize()}");

			Assert.IsTrue(anm.tracks.Count                           > 0, "No tracks were deserialized"     );
			Assert.IsTrue(anm.tracks[0].channels.Count               > 0, "No channels were deserialized"   );
			Assert.IsTrue(anm.tracks[0].channels[0].keyframes.Length > 0, "No keyframes were deserialized"  );

			Assert.IsTrue(anm.tracks[0].channels[0].keyframes.Length == 2, "More than 2 keyframes were deserialized");
		}

		[TestMethod]
		public void TestDeserializeAllKeyframes()
		{
			string filePath = k_ExampleActionPath;
			Anm anm;
			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				anm = m_Serializer.Deserialize<Anm>(file);
			}

			Console.WriteLine($"{filePath}: {anm.Summarize()}");

			Assert.IsTrue(anm.tracks.Count                           > 0, "No tracks were deserialized"     );
			Assert.IsTrue(anm.tracks[0].channels.Count               > 0, "No channels were deserialized"   );
			Assert.IsTrue(anm.tracks[0].channels[0].keyframes.Length > 0, "No keyframes were deserialized"  );
			Assert.IsTrue(anm.tracks.Count                           > 1, "Only 1 track was deserialized"   );
			Assert.IsTrue(anm.tracks[0].channels.Count               > 1, "Only 1 channel was deserialized" );
			Assert.IsTrue(anm.tracks[0].channels[0].keyframes.Length > 1, "Only 1 keyframe was deserialized");
		}

		[TestMethod]
		public void TestSerializeAndDeserialize()
		{
			Anm anm = CreateTestAnm();

			// Serialize Anm
			m_Serializer.Serialize(m_Stream, anm);

			string summaryA = anm.Summarize();
			Console.WriteLine($"anm = {summaryA}");


			m_Stream.Position = 0; // Reset stream position


			// Deserialize Anm
			anm = m_Serializer.Deserialize<Anm>(m_Stream);

			string summaryB = anm.Summarize();
			Console.WriteLine($"anm = {summaryB}");

			Assert.AreEqual(summaryA, summaryB);
		}

		[TestMethod]
		public void TestAnmUnchanged()
		{
			string filePath = k_ExamplePosePath;
			Anm anm;

			// Deserialize File
			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				anm = m_Serializer.Deserialize<Anm>(file);
			}

			Console.WriteLine($"anm.signature = \"{anm.signature}\"");

			// Serialize Anm
			var tempFilePath = Path.GetTempFileName(); // Use a temporary file
			Console.WriteLine($"Using temporary file {tempFilePath}");
			try
			{
				using (var tempFile = new FileStream(tempFilePath, FileMode.Open, FileAccess.Write))
				{
					m_Serializer.Serialize(tempFile, anm);
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
