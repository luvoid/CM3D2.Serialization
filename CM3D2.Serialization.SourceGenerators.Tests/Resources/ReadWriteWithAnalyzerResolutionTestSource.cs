using CM3D2.Serialization;
using CM3D2.Serialization.Collections;
using CM3D2.Serialization.Types;
using System.Collections.Generic;

class ReadWriteWithAnalyzerResolutionTestSource : ICM3D2Serializable
{
	[DeepSerialized]
	public List<UnmanagedStruct> ListField = new();

	[DeepSerialized]
	public List<DeepSerializableStruct> DeepListField = new();

	[DeepSerialized]
	public List<DeepSerializableStruct> EnumerableField = new();

	[DeepSerializable]
	public struct DeepSerializableStruct
	{
		public string Field0;
		public int Field1;
	}

	public struct UnmanagedStruct
	{
		public float Field0;
		public int Field1;
	}

	void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
	{
		for (int i = 0; i < 10; i++)
		{
			reader.Read(out UnmanagedStruct unmanagedStruct);
			var value = unmanagedStruct;
			ListField.Add(value);
		}

		for (int i = 0; i < 10; i++)
		{
			DeepSerializableStruct deepSerializableStruct = new();
			reader.Read(out deepSerializableStruct.Field0);
			reader.Read(out deepSerializableStruct.Field1);
			DeepListField[i] = deepSerializableStruct;
		}

		for (int i = 0; i < 10; i++)
		{
			DeepSerializableStruct readItem = default;
			reader.Read(out readItem.Field0);
			reader.Read(out readItem.Field1);
			EnumerableField.Add(readItem);
		}
	}

	public void WriteWith(ICM3D2Writer writer)
	{
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


		foreach (var writeItem in EnumerableField)
		{
			writer.Write(writeItem.Field0);
			writer.Write(writeItem.Field1);
		}
	}
}