using CM3D2.Serialization.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CM3D2.Serialization.Collections
{
	/// <summary>
	/// A list of strings where that is prefixed by its length as an integer.
	/// </summary>
	public sealed class LengthPrefixedStringList : List<string>, ICM3D2Serializable
	{
		public Encoding Encoding = Encoding.UTF8;

		public LengthPrefixedStringList()
			: this(0)
		{ }

		public LengthPrefixedStringList(int size)
			: base(size)
		{ }

		public LengthPrefixedStringList(IList<string> list)
			: base(list)
		{ }

		void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
		{
			writer.Write(Count);
			foreach (string element in this)
			{
				writer.Write(element, Encoding);
			}
		}

		unsafe void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
		{
			this.Clear();
			reader.Read(out int length);
			this.Capacity = length;
			for (int i = 0; i < length; i++)
			{
				reader.Read(out string element, Encoding);
				this.Add(element);
			}
		}
	}
}
