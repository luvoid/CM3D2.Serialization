using CM3D2.Serialization.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace CM3D2.Serialization.Tests
{

	[TestClass]
	public class TestAnm : FileTests<Anm>
	{
		protected override Anm CreateExample()
		{
			Anm anm = new Anm();
			anm.tracks.Add(new Anm.Track());
			anm.tracks[0].path = "path/to/bone";
			anm.tracks[0].channels.Add(new Anm.Channel());
			anm.tracks[0].channels[0].keyframes = new Collections.LengthPrefixedArray<Anm.Keyframe>(4);

			return anm;
		}

		protected override string ExampleFilePath => k_ExamplePosePath;

		[TestMethod] public override void SerializeTest() => DoSerializeTest();
		[TestMethod] public override void DeserializeTest() => DoDeserializeTest();
		[TestMethod] public override void SerializeAndDeserializeTest() => DoSerializeAndDeserializeTest();
		[TestMethod] public override void UnchangedTest() => DoUnchangedTest();


		const string k_ExamplePosePath = "Resources/T-Pose.anm";
		const string k_ExampleActionPath = "Resources/push_ups.anm";
		//const string k_ExampleActionPath = "Resources/maid_stand01.anm";
		//const string k_ExampleActionPath = "Resources/dance_cm3d21_008_1oy_c01_02_f1.anm";

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
	}
}
