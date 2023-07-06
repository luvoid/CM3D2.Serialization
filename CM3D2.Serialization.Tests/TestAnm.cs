using CM3D2.Serialization.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace CM3D2.Serialization.Tests
{
	[TestClass]
	public class TestAnm : CommonTest
	{
		const string k_ExamplePosePath = "T-Pose.anm";
		//const string k_ExampleActionPath = "maid_stand01.anm";
		const string k_ExampleActionPath = "push_ups.anm";

		[TestMethod]
		public void TestSerializeAnm()
		{
			Anm anm = new Anm();

			m_Formatter.Serialize(m_Stream, anm);


			m_Stream.Position = 0; // Reset stream position


			byte[] result = ReadBytes();
			PrintBytes(result);
		}

		[TestMethod]
		public void TestDeserializeAnm()
		{
			string filePath = k_ExamplePosePath;
			Anm anm;
			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				anm = m_Formatter.Deserialize<Anm>(file);
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
				anm = m_Formatter.Deserialize<Anm>(file);
			}


			Console.WriteLine($"{filePath}: {anm.Summarize()}");

			Assert.IsTrue(anm.tracks.Count                           > 0, "No tracks were deserialized"     );
			Assert.IsTrue(anm.tracks[0].channels.Count               > 0, "No channels were deserialized"   );
			Assert.IsTrue(anm.tracks[0].channels[0].keyframes.Length > 0, "No keyframes were deserialized"  );

			Assert.IsTrue(anm.tracks[0].channels[0].keyframes.Length == 1, "More than 1 keyframe was deserialized");
		}

		[TestMethod]
		public void TestDeserializeAllKeyframes()
		{
			string filePath = k_ExampleActionPath;
			Anm anm;
			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				anm = m_Formatter.Deserialize<Anm>(file);
			}

			Console.WriteLine($"{filePath}: {anm.Summarize()}");

			Assert.IsTrue(anm.tracks.Count                           > 0, "No tracks were deserialized"     );
			Assert.IsTrue(anm.tracks[0].channels.Count               > 0, "No channels were deserialized"   );
			Assert.IsTrue(anm.tracks[0].channels[0].keyframes.Length > 0, "No keyframes were deserialized"  );
			Assert.IsTrue(anm.tracks.Count                           > 1, "Only 1 track was deserialized"   );
			Assert.IsTrue(anm.tracks[0].channels.Count               > 1, "Only 1 channel was deserialized" );
			Assert.IsTrue(anm.tracks[0].channels[0].keyframes.Length > 1, "Only 1 keyframe was deserialized");
		}
	}
}
