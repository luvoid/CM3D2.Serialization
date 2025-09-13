using CM3D2.Serialization.Collections;
using CM3D2.Serialization.Types;
using System;
using System.CodeDom;

namespace CM3D2.Serialization.Files;

public class Tex : ICM3D2Serializable
{
	public readonly string signature = "CM3D2_TEX";

	public int version;

	public string unknown = string.Empty;

	[FileVersionConstraint(1011)]
	public int uvRectCount;

	[FileVersionConstraint(1011)]
	[LengthDefinedBy(nameof(uvRectCount))]
	public LengthDefinedArray<Float4> uvRects = new();

	[FileVersionConstraint(1010)]
	public int width = -1;

	[FileVersionConstraint(1010)]
	public int height = -1;

	[FileVersionConstraint(1010)]
	public int format = -1;

	public int imageDataSize;

	[LengthDefinedBy(nameof(imageDataSize))]
	//<summary>Typically you'll want to pass the results of EncodeToPNG here and set format to ARGB32.</summary>
	public LengthDefinedArray<byte> imageData = new();

	public void WriteWith(ICM3D2Writer writer)
	{
		if (width >= 0 || height >= 0 || format >= 0)
		{
			version = uvRectCount >= 0 ? 1011 : 1010;

			if (width <= -1)
			{
				throw new InvalidOperationException("Format or Height is set but width is not");
			}
			if (height <= -1)
			{
				throw new InvalidOperationException("Format or Width is set but height is not");
			}
			if (format <= -1)
			{
				throw new InvalidOperationException("Width or Height is set but format is not");
			}
		}
		else
		{
			version = 1000;

			if (uvRectCount >= 0)
			{
				throw new InvalidOperationException("uvRectCount is set but width, height, and format are not defined.");
			}
			if (imageDataSize < 24)
			{
				throw new InvalidOperationException("imageDataSize is less than 24 bytes, which is not valid for version 1000.");
			}
		}

		writer.Write(signature);
		writer.Write(version);
		writer.Write(unknown);

		if (version >= 1011)
		{
			writer.Write(uvRectCount);
			uvRects.ValidateLength(uvRectCount, "uvRects", "uvRectCount");
			writer.Write(uvRects);
		}

		if (version >= 1010)
		{
			writer.Write(width);
			writer.Write(height);
			writer.Write(format);
		}

		writer.Write(imageDataSize);
		imageData.ValidateLength(imageDataSize, "imageData", "imageDataSize");
		writer.Write(imageData);
	}

	public void ReadWith(ICM3D2Reader reader)
	{
		reader.Read(out var text);
		if (text != signature)
		{
			throw new FormatException(string.Concat("Expected signature \"", signature, "\" but instead found \"", text, "\""));
		}

		reader.Read(out version);
		reader.Read(out unknown);

		if (version >= 1011)
		{
			reader.Read(out uvRectCount);
			uvRects.SetLength(uvRectCount);
			reader.Read(ref uvRects);
		}

		if (version >= 1010)
		{
			reader.Read(out width);
			reader.Read(out height);
			reader.Read(out format);
		}

		reader.Read(out imageDataSize);
		imageData.SetLength(imageDataSize);
		reader.Read(ref imageData);

		if (version == 1000)
		{
			if (imageData.Length >= 24)
			{
				width = (imageData[16] << 24) | (imageData[17] << 16) | (imageData[18] << 8) | imageData[19];
				height = (imageData[20] << 24) | (imageData[21] << 16) | (imageData[22] << 8) | imageData[23];
				format = 5;
			}
			else
			{
				throw new FormatException("imageData does not contain enough data to extract width and height for version 1000.");
			}
		}
	}
}