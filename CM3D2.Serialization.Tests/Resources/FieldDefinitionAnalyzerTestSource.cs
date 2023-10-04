using CM3D2.Serialization;
using CM3D2.Serialization.Collections;
using CM3D2.Serialization.Files;
using CM3D2.Serialization.Types;
using static CM3D2.Serialization.Files.Model;
using System;

class FieldDefinitionAnalyzerTestSource : ICM3D2Serializable
{
	public int IntField;
	public string StringField;

	public LengthDefinedList<Material> LengthUndefinedField = new();

	[LengthDefinedBy(nameof(StringField))]
	public LengthDefinedArray<Float2> InvalidLengthDefinedField0 = new();

	[LengthDefinedBy("MissingField")]
	public LengthDefinedArray<Float2> InvalidLengthDefinedField1 = new();

	[LengthDefinedBy(nameof(IntField))]
	public LengthDefinedArray<Float3> ValidLengthDefinedField = new();

	void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
	{
		// Missing LengthUndefinedField.SetLength()
		reader.Read(ref LengthUndefinedField);

		// Setting length with wrong value
		InvalidLengthDefinedField0.SetLength(IntField);
		reader.Read(ref InvalidLengthDefinedField0);

		// InvalidLengthDefinedField1.SetLength() call may be skipped
		if (StringField != string.Empty)
		{
			InvalidLengthDefinedField1.SetLength(IntField);
		}
		reader.Read(ref InvalidLengthDefinedField1);

		// No issue
		ValidLengthDefinedField.SetLength(IntField);
	}

	void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
	{
	}

}