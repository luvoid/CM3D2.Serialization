using CM3D2.Serialization.Files;
using CM3D2.Serialization.Types;
using CM3D2.Serialization.UnityEngine;
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
	public class MenuTests : FileTests<Menu>
	{
		protected override Menu CreateExample()
		{
			Menu menu = new();
			menu.version = 1000;
			menu.srcFileName = "test_menu.txt";
			menu.itemName = "Test Menu";
			menu.category = "test";
			menu.infoText = "A test menu";

			menu.commands.Add(new Menu.Command("command0", "arg1"));
			menu.commands.Add(new Menu.Command("command1", "arg1", "arg2"));

			menu.UpdateBodySize();

			return menu;
		}

		protected override string ExampleFilePath => k_ExampleMenuPath;

		const string k_ExampleMenuPath = "Resources/dress236_onep_i_.menu";

		[TestMethod] public override void SerializeTest() => DoSerializeTest();
		[TestMethod] public override void DeserializeTest() => DoDeserializeTest();
		[TestMethod] public override void SerializeAndDeserializeTest() => DoSerializeAndDeserializeTest();
		[TestMethod] public override void UnchangedTest() => DoUnchangedTest();
	}
}
