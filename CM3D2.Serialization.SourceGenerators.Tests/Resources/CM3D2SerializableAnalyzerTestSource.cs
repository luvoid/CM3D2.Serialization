﻿using CM3D2.Serialization;
using CM3D2.Serialization.Collections;
using CM3D2.Serialization.Types;
using System.Collections.Generic;
using System.Xml.Linq;

class InheritedClass
{
	public bool InheritedField;
}

class CM3D2SerializableAnalyzerTestSource : InheritedClass, ICM3D2Serializable
{
	public int IntField;

	public string StringField;

	public Float3 UnmanagedStructField;

	public ManagedStruct ManagedStructField;

	[DeepSerialized]
	public ManagedStruct DeepSerializedField;

	public Float4[] ArrayField;

	[DeepSerialized]
	public List<Float4> ListField = new();

	[DeepSerialized]
	public List<DeepSerializableStruct> DeepListField = new();

	[LengthDefinedBy(nameof(IntField))]
	public LengthDefinedArray<Float3> LengthDefinedArrayField = new();


	public struct ManagedStruct
	{
		public string Field0;
		public int Field1;
	}

	[DeepSerializable]
	public struct DeepSerializableStruct
	{
		public string Field0;
		public int Field1;
	}

	void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
	{
		reader.Read(out IntField);

		var x = (IReadOnlyCollection<Float4>)ArrayField;

		// Missing read
		// reader.Read(out StringField);

		reader.Read(out UnmanagedStructField);
		
		// Attempt to read a managed struct as an unmanaged struct
		reader.Read(out ManagedStructField);

		reader.Read(out DeepSerializedField.Field0);
		// Missing read
		// reader.Read(out DeepSerializedField.Field1);

		for (int i=0; i < 10; i++)
		{
			reader.Read(out Float4 float4);
			ListField.Add(float4);
		}

		for (int i = 0; i < 10; i++)
		{
			reader.Read(out DeepSerializableStruct float4);
			DeepListField.Add(float4);
		}

		LengthDefinedArrayField.SetLength(IntField);
		reader.Read(ref LengthDefinedArrayField);
	}

	public void WriteWith(ICM3D2Writer writer)
	{
		// Write out of order
		writer.Write(StringField);

		// Write out of order
		writer.Write(IntField);

		writer.Write(StringField);

		writer.Write(UnmanagedStructField);

		// Attempt to write a managed struct as an unmanaged struct
		writer.Write(ManagedStructField);

		// Missing write
		// writer.Write(DeepSerializedField.Field0);
		writer.Write(DeepSerializedField.Field1);


		for (int i = 0; i < 10; i++)
		{
			writer.Write(ListField[i]);
		}


		for (int i = 0; i < 10; i++)
		{
			var element = DeepListField[i];
			writer.Write(element.Field0);
			writer.Write(element.Field1);
		}

		//foreach (var element in DeepListField)
		//{
		//	writer.Write(element.Field0);
		//	writer.Write(element.Field1);
		//}

		LengthDefinedArrayField.ValidateLength(IntField);
		writer.Write(LengthDefinedArrayField);
	}

}