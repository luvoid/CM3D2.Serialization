using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using CM3D2.Serialization.Collections;
using CM3D2.Serialization.Types;

namespace CM3D2.Serialization.Files
{
    // ImportCM.LoadSkinMesh_R

    /// <remarks>
    ///	If <c>2100 &lt; version &lt; 2200</c>, COM3D2.5 will throw an error if the
    ///	filename does not start with "crc_", "crx_", or "gp03_".
    /// </remarks>
    public partial class Model : ICM3D2Serializable, ISummarizable
	{
		public readonly string signature = "CM3D2_MESH";

		public int version = (int)FileVersions.CM3D2;

		/// <summary>
		/// The name of the model.
		/// </summary>
		public string modelName;

		/// <summary>
		/// The name of the object that holds the mesh. There must exist a child with this name.
		/// (A SkinnedMeshRenderer component will be added when loaded by the game.)
		/// </summary>
		public string meshObjectName;

		public int childCount;

		/// <summary>
		/// Minimum File Version: 2104 <br/>
		/// Maximum File Version: 2200 (exclusive)
		/// </summary>
		[FileVersionConstraint(2104, 2200)]
		public string shadowCastingMode;

		[LengthDefinedBy(nameof(childCount))]
		public LengthDefinedList<ChildName> childNames = new();

		[AutoCM3D2Serializable]
		public partial struct ChildName : ICM3D2Serializable
		{
			public string name;
			public bool isSclBone;
		}

		[LengthDefinedBy(nameof(childCount))]
		public LengthDefinedArray<int> childParents = new();

		[LengthDefinedBy(nameof(childCount))]
		public List<LocalTransform> childLocalTransforms = new();

		public struct LocalTransform
		{
			public Float3 localPosition;
			public Float4 localRotation;

			/// <summary> Minimum File Version: 2001 </summary>
			[FileVersionConstraint(FileVersions.COM3D2_1)]
			public BoolPrefixedNullable<Float3> localScale;
		}

		public int vertexCount;
		public int submeshCount;
		public int bindPoseCount;

		[LengthDefinedBy(nameof(bindPoseCount))]
		public LengthDefinedStringList boneUseNames = new();

		[LengthDefinedBy(nameof(bindPoseCount))]
		public LengthDefinedArray<Float4x4> bindPoses = new();

		[LengthDefinedBy(nameof(vertexCount))]
		public LengthDefinedArray<Vertex> vertices = new();

		public struct Vertex
		{
			public Float3 position;
			public Float3 normal;
			public Float2 uv;
		}

		public LengthPrefixedArray<Float4> tangents = new();

		[LengthDefinedBy(nameof(vertexCount))]
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

		[LengthDefinedBy(nameof(submeshCount))]
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
			childNames.SetLength(childCount);
			reader.Read(ref childNames);

			reader.DebugLog("childParents");
			childParents.SetLength(childCount);
			reader.Read(ref childParents);

			reader.DebugLog("childLocalTransforms");
			childLocalTransforms.Clear();
			childLocalTransforms.Capacity = childNames.Count;
			for (int i = 0; i < childNames.Count; i++)
			{
				LocalTransform localTransform = default;
				reader.Read(out localTransform.localPosition);
				reader.Read(out localTransform.localRotation);
				if (version >= (int)FileVersions.COM3D2_1)
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
			boneUseNames.SetLength(bindPoseCount);
			reader.Read(ref boneUseNames);

			reader.DebugLog("bindPoses");
			bindPoses.SetLength(bindPoseCount);
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

			childNames.ValidateLength(childCount, nameof(childNames), nameof(childCount));
			writer.Write(childNames);

			childParents.ValidateLength(childCount, nameof(childParents), nameof(childCount));
			writer.Write(childParents);

			if (childLocalTransforms.Count != childNames.Count) throw new InvalidOperationException(
				$"{nameof(childLocalTransforms)}.{nameof(childLocalTransforms.Count)} must be equal to {nameof(childNames)}.{nameof(childNames.Count)}");
			foreach (var localTransform in childLocalTransforms)
			{
				writer.Write(localTransform.localPosition);
				writer.Write(localTransform.localRotation);
				if (version >= (int)FileVersions.COM3D2_1)
				{
					writer.Write(localTransform.localScale);
				}
			}

			vertexCount = vertices.Length;
			writer.Write(vertexCount);

			submeshCount = submeshTriangles.Count;
			writer.Write(submeshCount);

			boneUseNames.ValidateLength(bindPoseCount, nameof(bindPoses), nameof(bindPoseCount));
			writer.Write(boneUseNames);

			bindPoses.ValidateLength(bindPoseCount, nameof(bindPoses), nameof(bindPoseCount));
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