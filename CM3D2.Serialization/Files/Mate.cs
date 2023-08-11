using CM3D2.Serialization.Types;
using System;
using System.Text;

namespace CM3D2.Serialization.Files
{
	// ImportCM.LoadMaterial
	public class Mate : ICM3D2Serializable, ISummarizable
	{
		public readonly string signature = "CM3D2_MATERIAL";
		public int version = 1000;
		public string name = "";
		public Material material = new();

		void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
		{
			reader.Read(out string foundSignature);
			if (foundSignature != signature) throw new FormatException($"Expected signature \"{signature}\" but instead found \"{foundSignature}\"");

			reader.Read(out version);
			reader.Read(out name);
			reader.Read(out material);
		}

		void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
		{
			writer.Write(signature);
			writer.Write(version);
			writer.Write(name);
			writer.Write(material);
		}

		public string Summarize()
		{
			return $"{{ {signature} v{version} \"{name}\" {material.Summarize()} }}";
		}
	}
}
