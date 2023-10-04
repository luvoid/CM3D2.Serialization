using System;
using System.Collections.Generic;
using System.Text;

namespace CM3D2.Serialization.Types
{
    // ImportCM.ReadMaterial
    public partial class Material : ICM3D2Serializable, ISummarizable
	{
		public string name;
		public string shaderName;
		public string shaderFilename;

		public List<Property> properties = new();

		public abstract class Property : ICM3D2Serializable
		{
			public abstract string type { get; }
			public string name { get; set; }

			void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
			{
				reader.Read(out string foundType);
				if (foundType != type) throw new FormatException($"Property expected type \"{type}\" but instead found \"{foundType}\"");
				reader.Read(out string foundName);
				name = foundName;
				ReadWith(reader);
			}

			void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
			{
				writer.Write(type);
				writer.Write(name);
				WriteWith(writer);
			}

			protected abstract void ReadWith(ICM3D2Reader reader);
			protected abstract void WriteWith(ICM3D2Writer writer);
		}

		public partial class TexProperty : Property
		{
			public static readonly string Type = "tex";
			public override string type => Type;

			/// <summary>
			/// Expecting "null", "tex2d", or "texRT", but other values are not invalid.
			/// </summary>
			public string subtag;

			public Tex2DSubProperty? tex2d;
			public TexRTSubProperty? texRT;

			[AutoCM3D2Serializable]
			public partial struct Tex2DSubProperty : ICM3D2Serializable
			{
				public static readonly string Subtype = "tex2d";
				public string name;
				public string path;
				public Float2 offset;
				public Float2 scale;
			}

			[AutoCM3D2Serializable]
			public partial struct TexRTSubProperty : ICM3D2Serializable
			{
				public static readonly string Subtype = "texRT";
				public string discardedStr1;
				public string discardedStr2;
			}

			protected override void ReadWith(ICM3D2Reader reader)
			{
				reader.Read(out subtag);
				if (subtag == Tex2DSubProperty.Subtype)
				{
					reader.Read(out tex2d);
				}
				else if (subtag == TexRTSubProperty.Subtype)
				{
					reader.Read(out texRT);
				}
			}

			protected override void WriteWith(ICM3D2Writer writer)
			{
				writer.Write(subtag);
				if (subtag == Tex2DSubProperty.Subtype)
				{
					writer.Write(tex2d);
				}
				else if (subtag == TexRTSubProperty.Subtype)
				{
					writer.Write(texRT);
				}
			}
		}
	
		public class ColProperty : Property
		{
			public static readonly string Type = "col";
			public override string type => Type;

			public Float4 color;

			protected override void ReadWith(ICM3D2Reader reader)
			{
				reader.Read(out color);
			}

			protected override void WriteWith(ICM3D2Writer writer)
			{
				writer.Write(color);
			}
		}

		public class VecProperty : Property
		{
			public static readonly string Type = "vec";
			public override string type => Type;

			public Float4 vector;

			protected override void ReadWith(ICM3D2Reader reader)
			{
				reader.Read(out vector);
			}

			protected override void WriteWith(ICM3D2Writer writer)
			{
				writer.Write(vector);
			}
		}

		public class FProperty : Property
		{
			public static readonly string Type = "f";
			public override string type => Type;

			public float number;

			protected override void ReadWith(ICM3D2Reader reader)
			{
				reader.Read(out number);
			}

			protected override void WriteWith(ICM3D2Writer writer)
			{
				writer.Write(number);
			}
		}

		public readonly string endTag = "end";

		void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
		{
			reader.Read(out name);
			reader.Read(out shaderName);
			reader.Read(out shaderFilename);

			while (true)
			{
				string propType = reader.PeekString();
				Property prop;
				if (propType == TexProperty.Type)
				{
					reader.Read(out TexProperty newProp);
					prop = newProp;
				}
				else if (propType == ColProperty.Type)
				{
					reader.Read(out ColProperty newProp);
					prop = newProp;
				}
				else if (propType == VecProperty.Type)
				{
					reader.Read(out VecProperty newProp);
					prop = newProp;
				}
				else if (propType == FProperty.Type)
				{
					reader.Read(out FProperty newProp);
					prop = newProp;
				}
				else if (propType == "end")
				{
					break;
				}
				else
				{
					throw new FormatException($"Found property with invalid type '{propType}'");
				}
				properties.Add(prop);
			}

			reader.Read(out string foundEndTag);
			if (foundEndTag != endTag) throw new FormatException($"Expected endTag \"{endTag}\" but instead found \"{foundEndTag}\"");
		}

		void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
		{
			writer.Write(name);
			writer.Write(shaderName);
			writer.Write(shaderFilename);
			foreach (var prop in properties)
			{
				writer.Write(prop);
			}
			writer.Write(endTag);
		}
	
		public string Summarize()
		{
			StringBuilder stringBuilder = new();
			stringBuilder.Append($"{{\"{name}\" \"{shaderName}\" Property[{properties.Count}]}}");
			return stringBuilder.ToString();
		}
	}

	public partial class Material
	{
		/*
		private void ReadMaterial(BinaryReader r)
		{
			while (true)
			{
				string tag;
				string slot;
				string subtag;
				tag = r.ReadString();
				if (tag == "end")
				{
					return material;
				}
				else
				{
					slot = r.ReadString();
					if (tag == "tex")
					{
						subtag = r.ReadString();
						if (subtag == "null")
							material.SetTexture(slot, (Texture)null);
						else if (subtag == "tex2d")
						{
							string str5 = r.ReadString();
							r.ReadString();
							Texture2D texture = ImportCM.CreateTexture(AutoConverterManaged.CheckMateTexName(str5 + ".tex", material));
							texture.name = str5;
							texture.wrapMode = TextureWrapMode.Clamp;
							material.SetTexture(slot, (Texture)texture);
							bodyskin?.listDEL.Add((UnityEngine.Object)texture);
							Vector2 vector2_1;
							vector2_1.x = r.ReadSingle();
							vector2_1.y = r.ReadSingle();
							material.SetTextureOffset(slot, vector2_1);
							Vector2 vector2_2;
							vector2_2.x = r.ReadSingle();
							vector2_2.y = r.ReadSingle();
							material.SetTextureScale(slot, vector2_2);
						}
						else if (subtag == "texRT")
						{
							r.ReadString();
							r.ReadString();
						}
					}
					else if (tag == "col")
					{
						Color color;
						color.r = r.ReadSingle();
						color.g = r.ReadSingle();
						color.b = r.ReadSingle();
						color.a = r.ReadSingle();
						material.SetColor(slot, color);
					}
					else if (tag == "vec")
					{
						Vector4 vector4;
						vector4.x = r.ReadSingle();
						vector4.y = r.ReadSingle();
						vector4.z = r.ReadSingle();
						vector4.w = r.ReadSingle();
						material.SetVector(slot, vector4);
					}
					else if (tag == "f")
					{
						float num = r.ReadSingle();
						material.SetFloat(slot, num);
					}
					else
						Debug.LogError((object)("マテリアルが読み込めません。不正なマテリアルプロパティ型です " + tag));
				}
			}
		}*/
	}
}
