using CM3D2.Serialization;
using CM3D2.Serialization.Collections;
using CM3D2.Serialization.Types;

class UnmanagedSerializableAnalyzerTestSource
{
	[UnmanagedSerializable]
	struct GoodStruct
	{
		public int Field0;
		public int Field1;
	}

	[UnmanagedSerializable, DeepSerializable]
	struct BadStruct0
	{
		public string StringField;
		public int IntField;
		public int PrivateField;
		public GoodStruct GoodStructField;
	}

	[UnmanagedSerializable]
	struct BadStruct1 : ICM3D2Serializable
	{
		public BadStruct0 BadStruct0Field;
		public void ReadWith(ICM3D2Reader reader) => throw new System.NotImplementedException();
		public void WriteWith(ICM3D2Writer writer) => throw new System.NotImplementedException();
	}

	struct InterfaceSerializableStruct : ICM3D2Serializable
	{
		public int Field0;
		public void ReadWith(ICM3D2Reader reader) => throw new System.NotImplementedException();
		public void WriteWith(ICM3D2Writer writer) => throw new System.NotImplementedException();
	}

	LengthPrefixedArray<GoodStruct> goodArrayField;
	LengthPrefixedArray<InterfaceSerializableStruct> badArrayField;
}


