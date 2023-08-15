using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using CM3D2.Serialization.Collections;
using CM3D2.Serialization.Structs;
using CM3D2.Serialization.Types;

namespace CM3D2.Serialization.Files
{
	// ImportCM.LoadSkinMesh_R
	public partial class Model : ICM3D2Serializable, ISummarizable
	{
		public readonly string signature = "CM3D2_MESH";
		public int version = 1000;

		/// <summary>
		/// The name of the model.
		/// </summary>
		public string modelName;

		/// <summary>
		/// The name of the object that holds the mesh. There must exist a child with this name.
		/// (A SkinnedMeshRenderer component will be added when loaded by the game.)
		/// </summary>
		public string meshObjectName;

		public LengthPrefixedList<ChildName> childNames = new();

		[AutoCM3D2Serializable]
		public partial struct ChildName : ICM3D2Serializable
		{
			public string name;
			public bool isSclBone;
		}

		/// <remarks> Length defined by childNames.Count </remarks>
		public LengthDefinedArray<int> childParents = new();

		/// <remarks> Length defined by childNames.Count </remarks>
		public List<LocalTransform> childLocalTransforms = new();

		public struct LocalTransform
		{
			public Float3 localPosition;
			public Float4 localRotation;

			/// <summary> Only present in version >= 2001 </summary>
			/// <remarks> In versions before 2001, not even the prefixed bool is present. <remarks>
			public BoolPrefixedNullable<Float3> localScale;
		}

		public int vertexCount;
		public int submeshCount;
		public LengthPrefixedStringList boneUseNames = new();

		/// <remarks> Length defined by boneUseNames.Count </remarks>
		public LengthDefinedArray<Float4x4> bindPoses = new();

		/// <remarks> Length defined by vertexCount </remarks>
		public LengthDefinedArray<Vertex> vertices = new();

		public struct Vertex
		{
			public Float3 position;
			public Float3 normal;
			public Float2 uv;
		}

		public LengthPrefixedArray<Float4> tangents = new();

		/// <remarks> Length defined by vertexCount </remarks>
		public LengthDefinedArray<BoneWeight> boneWeights = new();

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct BoneWeight
		{
			public ushort boneIndex0;
			public ushort boneIndex1;
			public ushort boneIndex2;
			public ushort boneIndex3;
			public float weight0;
			public float weight1;
			public float weight2;
			public float weight3;
		}

		// ImportCM.cs:255

		/// <remarks> Length defined by submeshCount </remarks>
		public LengthDefinedList<LengthPrefixedArray<ushort>> submeshTriangles = new();

		public LengthPrefixedList<Material> materials = new();


		// Extra Blocks
		public List<IBlock> blocks = new();

		public readonly string endTag = "end";

		public interface IBlock : ICM3D2Serializable
		{
			public string tag { get; }
		}

		[AutoCM3D2Serializable]
		public partial class EmptyBlock : IBlock
		{
			public string _tag;
			public string tag { get => _tag; set => _tag = value; }
		}

		public class MorphBlock : IBlock
		{
			public static string Tag = "morph";
			public string tag => Tag;
			public Morph morph = new();
			void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
			{
				reader.Read(out string foundTag);
				if (foundTag != Tag) throw new FormatException($"MorphBlock expected tag \"{Tag}\" but instead found \"{foundTag}\"");
				reader.Read(out morph);
			}

			void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
			{
				writer.Write(tag);
				writer.Write(morph);
			}
		}


		void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
		{
			reader.Read(out string foundSignature);
			if (foundSignature != signature) throw new FormatException($"Expected signature \"{signature}\" but instead found \"{foundSignature}\"");

			reader.Read(out version);
			reader.Read(out modelName);
			reader.Read(out meshObjectName);

			reader.DebugLog("childNames");
			reader.Read(out childNames);

			reader.DebugLog("childParents");
			childParents.SetLength(childNames.Count);
			reader.Read(ref childParents);

			reader.DebugLog("childLocalTransforms");
			childLocalTransforms.Clear();
			childLocalTransforms.Capacity = childNames.Count;
			for (int i = 0; i < childNames.Count; i++)
			{
				LocalTransform localTransform = default;
				reader.Read(out localTransform.localPosition);
				reader.Read(out localTransform.localRotation);
				if (version >= 2001)
				{
					reader.Read(out localTransform.localScale);
				}
				childLocalTransforms.Add(localTransform);
			}

			reader.DebugLog("vertexCount");
			reader.Read(out vertexCount);

			reader.DebugLog("submeshCount");
			reader.Read(out submeshCount);

			reader.DebugLog("vertexGroupNames");
			reader.Read(out boneUseNames);

			reader.DebugLog("bindPoses");
			bindPoses.SetLength(boneUseNames.Count);
			reader.Read(ref bindPoses);

			reader.DebugLog("vertices");
			vertices.SetLength(vertexCount);
			reader.Read(ref vertices);

			reader.DebugLog("tangents");
			reader.Read(out tangents);

			reader.DebugLog("boneWeights");
			boneWeights.SetLength(vertexCount);
			reader.Read(ref boneWeights);

			reader.DebugLog($"submeshTriangles[{submeshCount}]");
			submeshTriangles.SetLength(submeshCount);
			reader.Read(ref submeshTriangles);
			
			reader.Read(out materials);

			string peekTag;
			while ((peekTag = reader.PeekString()) != endTag) 
			{
				IBlock block;
				if (peekTag == "morph")
				{
					reader.Read(out MorphBlock morphBlock);
					block = morphBlock;
				}
				else
				{
					reader.Read(out EmptyBlock emptyBlock);
					block = emptyBlock;
				}
				blocks.Add(block);
			}

			reader.Read(out string foundEndTag);
			if (foundEndTag != endTag) throw new FormatException($"Expected endTag \"{endTag}\" but instead found \"{foundEndTag}\"");
		}

		void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
		{
			writer.Write(signature);
			writer.Write(version);
			writer.Write(modelName);
			writer.Write(meshObjectName);
			writer.Write(childNames);

			childParents.ValidateLength(childNames.Count, $"{nameof(childNames)}.{nameof(childNames.Count)}");
			writer.Write(childParents);

			if (childLocalTransforms.Count != childNames.Count) throw new InvalidOperationException(
				$"{nameof(childLocalTransforms)}.{nameof(childLocalTransforms.Count)} must be equal to {nameof(childNames)}.{nameof(childNames.Count)}");
			foreach (var localTransform in childLocalTransforms)
			{
				writer.Write(localTransform.localPosition);
				writer.Write(localTransform.localRotation);
				if (version >= 2001)
				{
					writer.Write(localTransform.localScale);
				}
			}

			vertexCount = vertices.Length;
			writer.Write(vertexCount);

			submeshCount = submeshTriangles.Count;
			writer.Write(submeshCount);

			writer.Write(boneUseNames);

			bindPoses.ValidateLength(boneUseNames.Count, nameof(bindPoses), $"{nameof(boneUseNames)}.{nameof(boneUseNames.Count)}");
			writer.Write(bindPoses);

			vertices.ValidateLength(vertexCount);
			writer.Write(vertices);

			writer.Write(tangents);

			boneWeights.ValidateLength(vertexCount, nameof(boneWeights), $"{nameof(vertices)}.{nameof(vertices.Length)}");
			writer.Write(boneWeights);

			submeshTriangles.ValidateLength(submeshCount);
			writer.Write(submeshTriangles);

			writer.Write(materials);
			
			foreach (var block in blocks)
			{
				writer.Write(block);
			}
			writer.Write(endTag);
		}

		public string Summarize()
		{
			StringBuilder stringBuilder = new();
			stringBuilder.Append($"{{ {signature} v{version} \"{modelName}\" ChildName[{childNames.Count}] Vertex[{vertexCount}] IBlock[{blocks.Count}] }}");
			return stringBuilder.ToString();
		}
	}
}
