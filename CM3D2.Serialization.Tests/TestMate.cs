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
	[TestClass]
	public class TestMate : FileTests<Mate>
	{
		protected override Mate CreateExample()
		{
			Mate mate = new Mate();
			mate.name = "test_mate";
			mate.material.name = "Test_Material";
			mate.material.shaderName = "Shader_Name";
			mate.material.shaderFilename = "Shader_Filename";
			mate.material.properties.Add(new Material.TexProperty());
			mate.material.properties.Add(new Material.ColProperty());
			mate.material.properties.Add(new Material.VecProperty());
			mate.material.properties.Add(new Material.FProperty  ());
			return mate;
		}

		protected override string ExampleFilePath => k_ExampleMatePath;

		const string k_ExampleMatePath = "Resources/face002_skin.mate";

		[TestMethod] public override void SerializeTest() => DoSerializeTest();
		[TestMethod] public override void DeserializeTest() => DoDeserializeTest();
		[TestMethod] public override void SerializeAndDeserializeTest() => DoSerializeAndDeserializeTest();
		[TestMethod] public override void UnchangedTest() => DoUnchangedTest();
	}
}
