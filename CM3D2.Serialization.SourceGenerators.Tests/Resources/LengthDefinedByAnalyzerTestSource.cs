﻿using CM3D2.Serialization;
using CM3D2.Serialization.Collections;
using CM3D2.Serialization.Types;
using System;

class LengthDefinedByAnalyzerTestSource : ICM3D2Serializable
{
	public int IntField;
	public string StringField;

	public LengthDefinedList<LengthDefinedByAnalyzerTestSource> LengthUndefinedField = new();

	[LengthDefinedBy(nameof(StringField))]
	public LengthDefinedArray<float> InvalidLengthDefinedField0 = new();

	[LengthDefinedBy("MissingField")]
	public LengthDefinedArray<float> InvalidLengthDefinedField1 = new();

	[LengthDefinedBy(nameof(IntField))]
	public LengthDefinedArray<float> ValidLengthDefinedField = new();

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
		reader.Read(ref ValidLengthDefinedField);
	}

	void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
	{
		// Missing LengthUndefinedField.ValidateLength()
		writer.Write(LengthUndefinedField);

		// Validating length with wrong value
		InvalidLengthDefinedField0.ValidateLength(IntField);
		writer.Write(InvalidLengthDefinedField0);

		// InvalidLengthDefinedField1.ValidateLength() call may be skipped
		if (StringField != string.Empty)
		{
			InvalidLengthDefinedField1.ValidateLength(IntField);
		}
		writer.Write(InvalidLengthDefinedField1);

		// No issue
		ValidLengthDefinedField.ValidateLength(IntField, nameof(ValidLengthDefinedField), nameof(IntField));
		writer.Write(InvalidLengthDefinedField1);
	}

}