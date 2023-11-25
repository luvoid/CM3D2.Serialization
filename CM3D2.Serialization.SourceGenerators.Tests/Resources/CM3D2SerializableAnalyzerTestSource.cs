using CM3D2.Serialization;
using CM3D2.Serialization.Collections;
using CM3D2.Serialization.Types;
using System;
using System.Collections.Generic;

class CM3D2SerializableAnalyzerTestSource : ICM3D2Serializable
{
	public int IntField;

	public string StringField;

	public ManagedStruct[] FloatProperty { get; set; } = default;

	public UnmanagedStruct UnmanagedStructField;

	public ManagedStruct ManagedStructField;

	[DeepSerialized]
	public ManagedStruct DeepSerializedField;

	public UnmanagedStruct[] ArrayField;

	public UnmanagedStruct? NullableUnmanagedStructField;

	public InterfacedStruct? NullableInterfacedStructField;

	public Nullable<InterfacedStruct> NullableInterfacedStructField1;


	public struct UnmanagedStruct
	{
		public float Field0;
		public int Field1;
	}

	public struct ManagedStruct
	{
		public string Field0;
		public int Field1;
	}

	public struct InterfacedStruct : ICM3D2Serializable
	{
		public string Field0;
		public void ReadWith(ICM3D2Reader reader) => throw new System.NotImplementedException();
		public void WriteWith(ICM3D2Writer writer) => throw new System.NotImplementedException();
	}

	[DeepSerializable]
	public struct DeepSerializableStruct
	{
		public string Field0;
		public int Field1;
	}

	[DeepSerializable]
	public abstract class FakeDeepSerializableClass
	{
		public virtual string Property { get; }
		public string BadProperty { get; private set; }

		private int BadField;

		public string Field0;
		public int Field1;
	}

	[DeepSerializable]
	public class DerivedClass : FakeDeepSerializableClass
	{
		public override string Property { get; }
	}

	void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
	{ }

	public void WriteWith(ICM3D2Writer writer)
	{ }
}