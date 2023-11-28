using CM3D2.Serialization.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CM3D2.Serialization.Collections
{
	/// <summary>
	/// A list that is prefixed by its length as an integer.
	/// </summary>
	/// <remarks>
	/// If <typeparamref name="T"/> is an unmanaged struct, 
	/// consider using a <see cref="LengthPrefixedArray{T}"/> instead.
	/// </remarks>
	public sealed class LengthPrefixedList<T> : List<T>, ICM3D2Serializable
		where T : ICM3D2Serializable, new()
	{
		public LengthPrefixedList()
			: this(0)
		{ }

		public LengthPrefixedList(int size)
			: base(size)
		{ }

		public LengthPrefixedList(IEnumerable<T> collection)
			: base(collection)
		{ }

		void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
		{
			writer.Write(Count);
			foreach (T element in this)
			{
				element.WriteWith(writer);
			}
		}

		void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
		{
			this.Clear();
			reader.Read(out int length);
			this.Capacity = length;
			for (int i = 0; i < length; i++)
			{
				T element = new();
				element.ReadWith(reader);
				this.Add(element);
			}
		}
	}
}
