using CM3D2.Serialization.Files;
using CM3D2.Serialization.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CM3D2.Serialization.Tests
{
	[TestClass]
	public class ModelTests : FileTests<Model>
	{
		protected override Model CreateExample()
		{
			Model model = new Model();
			model.modelName = "test_model";
			model.materials.Add(new Material());
			return model;
		}

		protected override string ExampleFilePath => k_ExampleMatePath;

		const string k_ExampleMatePath = "Resources/body001.model";

		[TestMethod] public override void SerializeTest() => DoSerializeTest();
		[TestMethod] public override void DeserializeTest() => DoDeserializeTest();
		[TestMethod] public override void SerializeAndDeserializeTest() => DoSerializeAndDeserializeTest();
		[TestMethod] public override void UnchangedTest() => DoUnchangedTest();

	}
}
